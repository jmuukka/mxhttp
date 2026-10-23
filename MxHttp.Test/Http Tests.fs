module MxHttp.HttpTests

open System
open System.Net
open System.Net.Http
open System.Text
open System.Threading
open Xunit

let deserializers =
    Map.empty.Add(MediaType.applicationJson, Json.deserialize)

let responseMessage statusCode =
    new HttpResponseMessage(statusCode)

let withJsonStringContent content (responseMessage: HttpResponseMessage) =
    let json = Json.serialize content
    let mediaType = MediaType.toString MediaType.applicationJson
    responseMessage.Content <- new StringContent(json, Encoding.UTF8, mediaType)
    responseMessage.Content.Headers.ContentLength <- json.Length
    responseMessage

let fakeHttpSendWithResponseContent responseContent cancellationToken httpRequestMessage =
    responseMessage HttpStatusCode.OK
    |> withJsonStringContent responseContent
    |> Task.fromResult

let httpPost expectedResponseContent uri content =
    content
    |> RequestContent.string Json.serialize MediaType.applicationJson
    |> Request.post [Header.acceptJson] uri
    |> Http.Task.send (fakeHttpSendWithResponseContent expectedResponseContent) CancellationToken.None
    |> Task.result
    |> Result.bind (Response.parse<CustomerResponse> deserializers >> Result.mapError ResponseError)

[<Fact>]
let ``given content when post and parse then returns expected response content`` () =
    let resourceUri = Uri.absolute "http://localhost/api/customers"
    let expectedContent = { Id = Guid.NewGuid(); Name = "Any" }

    { Name = "Any" }
    |> httpPost expectedContent resourceUri
    |> Assert.okWith (fun actualContent -> Assert.Equal(expectedContent, actualContent))
