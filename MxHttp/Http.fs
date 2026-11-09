namespace MxHttp

open System.Threading
open System.Threading.Tasks
open System.Net.Http

[<NoComparison>]
type HttpError =
| RequestException of Request * exn
| ResponseError of ResponseError
| ParseError of ParseError

module Http =

    module Task =

        let sendRequest : HttpSendTask -> CancellationToken -> Request -> Task<Result<HttpResponseMessage, HttpError>> = fun httpSend cancellationToken request ->
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

        let send : HttpSendTask -> CancellationToken -> Request -> Task<Result<Response, HttpError>> = fun httpSend cancellationToken request ->
            sendRequest httpSend cancellationToken request
            |> TaskResult.bind (Response.Task.ofHttpResponseMessage request >> TaskResult.mapError ResponseError)

    module Async =

        let sendRequest : HttpSendAsync -> Request -> Async<Result<HttpResponseMessage, HttpError>> = fun httpSend request ->
            async {
                try
                    let! httpResponseMessage =
                        request
                        |> Request.toHttpRequestMessage
                        |> httpSend

                    return Ok httpResponseMessage
                with ex ->
                    return
                        (request, ex)
                        |> RequestException
                        |> Error
            }

        let send : HttpSendAsync -> Request -> Async<Result<Response, HttpError>> = fun httpSend request ->
            sendRequest httpSend request
            |> AsyncResult.bind (Response.Async.ofHttpResponseMessage request >> AsyncResult.mapError ResponseError)
