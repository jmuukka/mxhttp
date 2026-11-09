namespace MxHttp

open System.Net.Http

type [<NoComparison>] Request = {
    Method: HttpMethod
    ResourceUri: Uri
    Headers: Header list
    Content: Content option
}

module Request =

    val get: Header list -> Uri -> Request

    val query: Header list -> Uri -> Content -> Request

    val post: Header list -> Uri -> Content -> Request

    val put: Header list -> Uri -> Content -> Request

    val patch: Header list -> Uri -> Content -> Request

    val delete: Header list -> Uri -> Request

    val toHttpRequestMessage: Request -> HttpRequestMessage
