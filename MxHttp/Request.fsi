namespace MxHttp

open System.IO
open System.Net.Http

type [<NoComparison>] Request = {
    Method: HttpMethod
    ResourceUri: Uri
    Headers: Header list
    Content: RequestContent option
}
and RequestContent =
| StringContent of MediaType * string
| ByteArrayContent of byte array
| FormUrlEncodedContent of (Key * Value) list

module HttpMethod =

    val patch: HttpMethod
    
    //val query: HttpMethod

module RequestContent =

    val string: ('a -> string) -> MediaType -> 'a -> RequestContent

    val byteArray: byte array -> RequestContent

    val formUrlEncoded: (string * string) list -> RequestContent

module HttpContent =

    val ofString: MediaType -> string -> HttpContent

    val ofBytes: byte array -> HttpContent

    val ofKeyValuePairs: (Key * Value) list -> HttpContent

    val ofStream: MediaType -> Stream -> HttpContent

    val ofRequestContent: RequestContent -> HttpContent

module Request =

    val get: Header list -> Uri -> Request

    // val query: Header list -> Uri -> RequestContent -> Request

    val post: Header list -> Uri -> RequestContent -> Request

    val put: Header list -> Uri -> RequestContent -> Request

    val patch: Header list -> Uri -> RequestContent -> Request

    val delete: Header list -> Uri -> Request

module HttpRequestMessage =

    val ofHttpContent: HttpMethod -> Header list -> Uri -> HttpContent -> HttpRequestMessage

    val ofRequest: Request -> HttpRequestMessage
