namespace MxHttp

open System.Net.Http
open System.Threading
open System.Threading.Tasks

[<NoComparison>]
type HttpError =
| RequestException of Request * exn
| ResponseError of ResponseError
| ParseError of ParseError

module Http =

    module Task =

        val send: HttpSendTask -> CancellationToken -> Request -> Task<Result<Response, HttpError>>

    module Async =

        val send: HttpSendAsync -> Request -> Async<Result<Response, HttpError>>
