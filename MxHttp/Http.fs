namespace MxHttp

open System.Threading
open System.Threading.Tasks
open System.Net.Http

[<Measure>]
type ms

type Timeout = int<ms>

type HttpSend = CancellationToken -> HttpRequestMessage -> Task<HttpResponseMessage>

[<NoComparison>]
type HttpError =
| RequestException of Request * exn
| ResponseError of ResponseError

module Http =

    module Task =

        let sendRequest (httpSend: HttpSend) cancellationToken request =
            backgroundTask {
                try
                    let! httpResponseMessage =
                        request
                        |> Request.toHttpRequestMessage
                        |> httpSend cancellationToken

                    return Ok httpResponseMessage
                with ex ->
                    return
                        (request, ex)
                        |> RequestException
                        |> Error
            }

        let send httpSend cancellationToken request =
            sendRequest httpSend cancellationToken request
            |> TaskResult.bind (Response.Task.ofHttpResponseMessage request >> TaskResult.mapError ResponseError)

    module Async =

        let sendRequest (httpSend: HttpSend) request =
            async {
                try
                    let! cancellationToken = Async.CancellationToken
                    let! httpResponseMessage =
                        request
                        |> Request.toHttpRequestMessage
                        |> httpSend cancellationToken
                        |> Async.AwaitTask

                    return Ok httpResponseMessage
                with ex ->
                    return
                        (request, ex)
                        |> RequestException
                        |> Error
            }

        let send httpSend request =
            sendRequest httpSend request
            |> AsyncResult.bind (Response.Async.ofHttpResponseMessage request >> AsyncResult.mapError ResponseError)

    //module Sync =

    //    let send httpSend timeout cancellationToken request =
    //        let computation = Async.send httpSend request

    //        Async.RunSynchronously (computation, timeout / 1<ms>, cancellationToken)
