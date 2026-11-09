module MxHttp.AsyncPipelineTests

open System
open System.IO
open System.Net
open System.Net.Http
open System.Threading
//open System.Threading.Tasks
open Xunit

let deserializers =
    Map.empty.Add(MediaType.applicationJson, Json.deserialize)

let bytes = [|1uy; 0uy; 255uy|]

[<Fact>]
let ``given HTTP content stream when send HTTP request message then HTTP response message's content's stream contains expected data`` () =
    use httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
    httpResponseMessage.Content <- new ByteArrayContent(bytes)
    let httpSend = Http.Async.httpSendReturns httpResponseMessage
    use requestStream = new MemoryStream(bytes)
    let requestUri = Uri.absolute "https://localhost/api/customers"
    let headers = []

    let httpResponseMessage =
        HttpContent.ofStream MediaType.applicationOctetStream requestStream
        |> HttpRequestMessage.ofHttpContent HttpMethod.Post requestUri headers
        |> HttpClient.Async.send httpSend
        |> Async.run
        |> Result.ok

    use responseStream =
        HttpResponseContent.Async.readAsStream httpResponseMessage.Content
        |> Async.run
    let actualBytes = Http.readStreamAsByteArray 100 responseStream
    Assert.Equal<byte array>(bytes, actualBytes)

[<Fact>]
let ``given content when post and parse then returns expected response content`` () =
    let resourceUri = Uri.absolute "http://localhost/api/customers"
    let headers = [Header.acceptJson]
    let expectedContent = { Id = Guid.NewGuid(); Name = "Any" }
    let httpSend : HttpRequestMessage -> Async<Result<HttpResponseMessage, _>> = fun httpRequestMessage ->
        let httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        httpResponseMessage.RequestMessage <- httpRequestMessage
        httpResponseMessage.Content <-
            JsonHttpContent.ofObject<CustomerResponse> expectedContent
        httpResponseMessage
        |> Ok
        |> async.Return

    let actual =
        { Name = "Any" }
        |> JsonHttpContent.ofObject<CustomerRequest>
        |> HttpRequestMessage.ofHttpContent HttpMethod.Post resourceUri headers
        |> httpSend
        |> Async.run
        |> Result.ok
        |> HttpResponseMessage.httpContent
        |> HttpResponseContent.Async.parse<CustomerResponse> deserializers
        |> Async.run
        |> Result.ok

    Assert.Equal(expectedContent, actual)
