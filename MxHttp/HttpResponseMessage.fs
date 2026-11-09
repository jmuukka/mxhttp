namespace MxHttp

open System.Net
open System.Net.Http

module HttpResponseMessage =

    let statusCode : HttpResponseMessage -> HttpStatusCode = fun httpResponseMessage ->
        httpResponseMessage.StatusCode

    let headers : HttpResponseMessage -> Header list = fun httpResponseMessage ->
        Header.ofHttpHeaders httpResponseMessage.Headers

    let httpContent : HttpResponseMessage -> HttpContent = fun httpResponseMessage ->
        httpResponseMessage.Content
