namespace MxHttp

open System.Net.Http

module HttpMethod =

    let patch = HttpMethod("PATCH")
    
    let query = HttpMethod("QUERY")

module HttpRequestMessage =

    let create : HttpMethod -> Uri -> HttpRequestMessage = fun httpMethod resourceUri ->
        new HttpRequestMessage(httpMethod, Uri.toSystemUri resourceUri)

    let applyHeaders : Header list -> HttpRequestMessage -> HttpRequestMessage = fun headers httpRequestMessage ->
        HttpHeader.applyHeaders headers httpRequestMessage.Headers
        httpRequestMessage

    let replaceHttpContent : HttpContent -> HttpRequestMessage -> HttpRequestMessage = fun httpContent httpRequestMessage ->
        httpRequestMessage.Content <- httpContent
        httpRequestMessage

    let ofHttpContent : HttpMethod -> Uri -> Header list -> HttpContent -> HttpRequestMessage = fun httpMethod resourceUri headers httpContent ->
        create httpMethod resourceUri
        |> applyHeaders headers
        |> replaceHttpContent httpContent
