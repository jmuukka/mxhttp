namespace MxHttp

open System.Net.Http

module HttpMethod =

    val patch: HttpMethod
    
    val query: HttpMethod

module HttpRequestMessage =

    val create: HttpMethod -> Uri -> HttpRequestMessage

    val applyHeaders: Header list -> HttpRequestMessage -> HttpRequestMessage

    val ofHttpContent: HttpMethod -> Uri -> Header list -> HttpContent -> HttpRequestMessage
