namespace MxHttp

open System.Threading
open System.Threading.Tasks
open System.Net.Http

type HttpSendTask = CancellationToken -> HttpRequestMessage -> Task<HttpResponseMessage>
type HttpSendAsync = HttpRequestMessage -> Async<HttpResponseMessage>

module HttpClient =

    module Task =

        let send : HttpSendTask -> CancellationToken -> HttpRequestMessage -> Task<Result<HttpResponseMessage, exn>> = fun httpSend cancellationToken httpRequestMessage ->
            backgroundTask {
                try
                    let! httpResponseMessage =
                        httpSend cancellationToken httpRequestMessage

                    return Ok httpResponseMessage
                with ex ->
                    return Error ex
            }

    module Async =

        let send : HttpSendAsync -> HttpRequestMessage -> Async<Result<HttpResponseMessage, exn>> = fun httpSend httpRequestMessage ->
            async {
                try
                    let! httpResponseMessage =
                        httpSend httpRequestMessage

                    return Ok httpResponseMessage
                with ex ->
                    return Error ex
            }
