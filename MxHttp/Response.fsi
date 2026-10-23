namespace MxHttp

open System.Net
open System.Net.Http
open System.Threading.Tasks

type [<NoComparison>] Response = {
    StatusCode: HttpStatusCode
    Headers: Header list
    Content: ResponseContent option
    Request: Request
}
and ResponseContent = {
    Headers: Header list
    Data: ResponseContentData
}
and ResponseContentData =
| StringResponseContent of string
| ByteArrayResponseContent of byte array

type Deserializer<'a> = string -> 'a

[<NoComparison>]
type ResponseError =
| ResponseContentException of Request * HttpResponseMessage * exn
| NonSuccessHttpStatusCode of Response
| ParseError of Response * string
| DeserializeException of Response * exn

module Response =

    module Task =

        val ofHttpResponseMessage: Request -> HttpResponseMessage -> Task<Result<Response, ResponseError>>

    module Async =

        val ofHttpResponseMessage: Request -> HttpResponseMessage -> Async<Result<Response, ResponseError>>

    val parse<'a>: Map<MediaType, Deserializer<'a>> -> Response -> Result<'a, ResponseError>
