namespace MxHttp

open System.Net.Http
open System.Threading
open System.Threading.Tasks

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

        val send: HttpSend -> CancellationToken -> Request -> Task<Result<Response, HttpError>> 

    module Async =

        val send: HttpSend -> Request -> Async<Result<Response, HttpError>> 

    //module Sync =

    //    val send: HttpSend -> Timeout -> CancellationToken -> Request -> Result<Response, HttpError>
