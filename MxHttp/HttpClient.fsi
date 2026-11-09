namespace MxHttp

open System.Net.Http
open System.Threading
open System.Threading.Tasks

type HttpSendTask = CancellationToken -> HttpRequestMessage -> Task<HttpResponseMessage>
type HttpSendAsync = HttpRequestMessage -> Async<HttpResponseMessage>

module HttpClient =

    module Task =

        val send: HttpSendTask -> CancellationToken -> HttpRequestMessage -> Task<Result<HttpResponseMessage, exn>>

    module Async =

        val send: HttpSendAsync -> HttpRequestMessage -> Async<Result<HttpResponseMessage, exn>>
