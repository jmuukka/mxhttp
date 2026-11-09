module MxHttp.TaskPipelineTests

open System
open System.IO
open System.Net
open System.Net.Http
open System.Threading
open System.Threading.Tasks
open Xunit

let deserializers =
    Map.empty.Add(MediaType.applicationJson, Json.deserialize)

let bytes = [|1uy; 0uy; 255uy|]

[<Fact>]
let ``given HTTP content stream when send HTTP request message then HTTP response message's content's stream contains expected data`` () =
    use httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
    httpResponseMessage.Content <- new ByteArrayContent(bytes)
    let httpSend = Http.Task.httpSendReturns httpResponseMessage
    use cancellationTokenSource = new CancellationTokenSource()
    use requestStream = new MemoryStream(bytes)
    let requestUri = Uri.absolute "https://localhost/api/customers"
    let headers = []

    let httpResponseMessage =
        HttpContent.ofStream MediaType.applicationOctetStream requestStream
        |> HttpRequestMessage.ofHttpContent HttpMethod.Post requestUri headers
        |> HttpClient.Task.send httpSend cancellationTokenSource.Token
        |> Task.result
        |> Result.ok

    use responseStream =
        HttpResponseContent.Task.readAsStream httpResponseMessage.Content
        |> Task.result
    let actualBytes = Http.readStreamAsByteArray 100 responseStream
    Assert.Equal<byte array>(bytes, actualBytes)

[<Fact>]
let ``given content when post and parse then returns expected response content`` () =
    let resourceUri = Uri.absolute "http://localhost/api/customers"
    let headers = [Header.acceptJson]
    let expectedContent = { Id = Guid.NewGuid(); Name = "Any" }
    let httpSend : HttpRequestMessage -> Task<Result<HttpResponseMessage, _>> = fun httpRequestMessage ->
        let httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        httpResponseMessage.RequestMessage <- httpRequestMessage
        httpResponseMessage.Content <-
            JsonHttpContent.ofObject<CustomerResponse> expectedContent
        httpResponseMessage
        |> Ok
        |> Task.fromResult

    let actual =
        { Name = "Any" }
        |> JsonHttpContent.ofObject<CustomerRequest>
        |> HttpRequestMessage.ofHttpContent HttpMethod.Post resourceUri headers
        |> httpSend
        |> Task.result
        |> Result.ok
        |> HttpResponseMessage.httpContent
        |> HttpResponseContent.Task.parse<CustomerResponse> deserializers
        |> Task.result
        |> Result.ok

    Assert.Equal(expectedContent, actual)
