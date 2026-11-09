module MxHttp.HttpClientAsyncTests

open System
open System.Net
open System.Net.Http
open System.Threading
open Xunit

[<Fact>]
let ``given an HttpRequestMessage when send then returns an expected HttpResponseMessage`` () =
    use httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
    let httpSend = Http.Async.httpSendReturns httpResponseMessage

    let actual =
        Uri.absolute "https://localhost/api/customers"
        |> HttpRequestMessage.create HttpMethod.Post
        |> HttpClient.Async.send httpSend
        |> Async.run
        |> Result.ok

    Assert.Same(httpResponseMessage, actual)

[<Fact>]
let ``given HTTP send throws an exception when send then returns an error with the same exception type`` () =
    let httpSend req =
        async {
            raise (new TimeoutException())
            return! Http.Async.httpSendReturns (new HttpResponseMessage(HttpStatusCode.OK)) req
        }
    Uri.absolute "https://localhost/api/customers"
    |> HttpRequestMessage.create HttpMethod.Post
    |> HttpClient.Async.send httpSend
    |> Async.run
    |> Assert.errorWith (
        function
        | :? TimeoutException -> ()
        | ex -> failwithf "Expected a TimeoutException, but got %A" ex)
