namespace MxHttp

open System.Net.Http

type [<NoComparison>] Request = {
    Method: HttpMethod
    ResourceUri: Uri
    Headers: Header list
    Content: RequestContent option
}
and [<NoComparison>] RequestContent =
| StringContent of MediaType * string
| ByteArrayContent of byte array
| FormUrlEncodedContent of (Key * Value) list

module HttpMethod =

    val patch: HttpMethod

module Request =

    val get: Header list -> Uri -> Request

    val post: Header list -> Uri -> RequestContent -> Request

    val put: Header list -> Uri -> RequestContent -> Request

    val patch: Header list -> Uri -> RequestContent -> Request

    val delete: Header list -> Uri -> Request

    val toHttpRequestMessage: Request -> HttpRequestMessage

module RequestContent =

    val string: ('a -> string) -> MediaType -> 'a -> RequestContent

    val byteArray: byte array -> RequestContent

    val formUrlEncoded: (string * string) list -> RequestContent
