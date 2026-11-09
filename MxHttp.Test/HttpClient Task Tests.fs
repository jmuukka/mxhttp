module MxHttp.HttpClientTaskTests

open System
open System.Net
open System.Net.Http
open System.Threading
open Xunit

[<Fact>]
let ``given an HttpRequestMessage when send then returns an expected HttpResponseMessage`` () =
    use httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
    let httpSend = Http.Task.httpSendReturns httpResponseMessage
    use cancellationTokenSource = new CancellationTokenSource()

    let actual =
        Uri.absolute "https://localhost/api/customers"
        |> HttpRequestMessage.create HttpMethod.Post
        |> HttpClient.Task.send httpSend cancellationTokenSource.Token
        |> Task.result
        |> Result.ok

    Assert.Same(httpResponseMessage, actual)

[<Fact>]
let ``given a cancelled token when send then returns an error OperationCanceledException`` () =
    use responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
    let httpSend = Http.Task.httpSendReturns responseMessage
    use cancellationTokenSource = new CancellationTokenSource()
    cancellationTokenSource.Cancel()

    Uri.absolute "https://localhost/api/customers"
    |> HttpRequestMessage.create HttpMethod.Post
    |> HttpClient.Task.send httpSend cancellationTokenSource.Token
    |> Task.result
    |> Assert.errorWith (
        function
        | :? OperationCanceledException -> ()
        | ex -> failwithf "%A" ex)
