namespace MxHttp

open System.IO
open System.Net
open System.Net.Http
open System.Threading.Tasks

/// The Response to the Request.
type [<NoComparison>] Response = {
    /// The HTTP Status Code of the response.
    StatusCode: HttpStatusCode
    /// The response headers.
    Headers: Header list
    /// The response content when there is one.
    Content: ResponseContent option
    /// The Request which was sent to the HTTP server.
    Request: Request
}
/// The response content.
and ResponseContent = {
    /// The response content headers.
    Headers: Header list
    /// The actual data of the response content.
    Content: Content
}

[<NoComparison>]
type ResponseError =
| ResponseContentException of Request * HttpResponseMessage * exn
| NonSuccessHttpStatusCode of Response

module Response =

    module Task =

        val ofHttpResponseMessage: Request -> HttpResponseMessage -> Task<Result<Response, ResponseError>>

    module Async =

        val ofHttpResponseMessage: Request -> HttpResponseMessage -> Async<Result<Response, ResponseError>>
