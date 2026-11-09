module MxHttp.HttpAsyncTests

open System
open System.Net
open System.Net.Http
open System.Text
open System.Threading
open System.Threading.Tasks
open Xunit
open MxHttp

let responseMessage statusCode =
    new HttpResponseMessage(statusCode)

let withJsonStringContent content (responseMessage: HttpResponseMessage) =
    let json = Json.serialize content
    let mediaType = MediaType.toString MediaType.applicationJson
    responseMessage.Content <- new StringContent(json, Encoding.UTF8, mediaType)
    responseMessage.Content.Headers.ContentLength <- json.Length
    responseMessage

[<Fact>]
let ``given a Request without Content when send then returns correct Response`` () =
    let request = Request.delete [] (Uri.absolute "http://localhost/api/customers/123456789")
    let httpSend httpRequestMessage =
        Assert.sameRequest request httpRequestMessage

        responseMessage HttpStatusCode.NoContent
        |> async.Return

    request
    |> Http.Async.send httpSend
    |> Async.run
    |> Assert.okWith (fun response ->
        Assert.Equal(request, response.Request)
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode)
        Assert.Equal<Header list>([], response.Headers)
        Assert.Equal(None, response.Content)
    )

[<Fact>]
let ``given a Request with Content when send then returns correct Response`` () =
    let request =
        { Name = "Any" }
        |> Content.string Json.serialize MediaType.applicationJson
        |> Request.post [Header.acceptJson] (Uri.absolute "http://localhost/api/customers")
    let expectedResponseObject = { Id = Guid.NewGuid(); Name = "added" }
    let expectedJson = Json.serialize expectedResponseObject
    let expectedResponseContent = {
        Headers = [
            Key.ofString "Content-Type", [Value.ofString "application/json; charset=utf-8"]
            Key.ofString "Content-Length", [Value.ofString (string expectedJson.Length)]
        ]
        Content = StringContent (MediaType.applicationJson, expectedJson)
    }
    let httpSend httpRequestMessage =
        Assert.sameRequest request httpRequestMessage

        responseMessage HttpStatusCode.OK
        |> withJsonStringContent expectedResponseObject
        |> async.Return

    request
    |> Http.Async.send httpSend
    |> Async.run
    |> Assert.okWith (fun response ->
        Assert.Equal(request, response.Request)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode)
        Assert.Equal<Header list>([], response.Headers)
        Assert.Equal(Some expectedResponseContent, response.Content)
    )

[<Fact>]
let ``given a Request when send throws an exception then returns correct Error`` () =
    let request = Request.delete [] (Uri.absolute "http://localhost/api/customers/123456789")
    let httpSend httpRequestMessage =
        Assert.sameRequest request httpRequestMessage

        raise (TaskCanceledException())

    request
    |> Http.Async.send httpSend
    |> Async.run
    |> Assert.requestException<TaskCanceledException>
