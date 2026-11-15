namespace MxHttp

open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Threading.Tasks
open MxHttp

type [<NoComparison>] Response = {
    StatusCode: HttpStatusCode
    Headers: Header list
    Content: ResponseContent option
    Request: Request
}
and ResponseContent = {
    Headers: Header list
    Content: Content
}

[<NoComparison>]
type ResponseError =
| ResponseContentException of Request * HttpResponseMessage * exn
| NonSuccessHttpStatusCode of Response

module Response =

    module Content =

        let toResponseContent : Header list -> Content -> ResponseContent = fun headers content -> {
                Headers = headers
                Content = content
            }

        let tryMediaType : HttpContentHeaders -> MediaType option = fun httpContentHeaders ->
            let contentType = httpContentHeaders.ContentType
            if contentType = null then
                None
            else
                contentType.MediaType
                |> MediaType.ofString
                |> Some

        let isByteArray : MediaType -> bool = fun mediaType ->
            mediaType = MediaType.applicationOctetStream

        let getHeadersAndContentLength : HttpContent -> Header list * int64 = fun httpContent ->
            // Take the original headers.
            let headers = Header.ofHttpHeaders httpContent.Headers

            // ContentLength property has a side-effect when the header value is missing.
            // When it's read then it adds the Content-Length header value.
            // This is the reason we will access the ContentLength property
            // after copying the original header values.
            let lengthOrNull = httpContent.Headers.ContentLength

            headers, lengthOrNull.GetValueOrDefault()

        module Task =

            let readAsByteArray : HttpContent -> Task<Content> = fun httpContent ->
                backgroundTask {
                    let! bytes = httpContent.ReadAsByteArrayAsync()

                    return ByteArrayContent bytes
                }

            let readAsString : MediaType -> HttpContent -> Task<Content> = fun mediaType httpContent ->
                backgroundTask {
                    let! text = httpContent.ReadAsStringAsync()

                    return StringContent (mediaType, text)
                }

            let toContent : HttpContent -> Task<Content> = fun httpContent ->
                match tryMediaType httpContent.Headers with
                | Some mediaType ->
                    if isByteArray mediaType
                    then readAsByteArray httpContent
                    else readAsString mediaType httpContent
                | None ->
                    readAsByteArray httpContent

            let tryReadResponseContent : HttpResponseMessage -> Task<Result<ResponseContent option, exn>> = fun httpResponseMessage ->
                backgroundTask {
                    try
                        let httpContent = httpResponseMessage.Content

                        match getHeadersAndContentLength httpContent with
                        | [], 0L ->
                            return Ok None
                        | headers, length ->
                            let! content = toContent httpContent
                            let responseContent = toResponseContent headers content
                            return Ok (Some responseContent)
                    with ex ->
                        return Error ex
                }

        module Async =

            let readAsByteArray : HttpContent -> Async<Content> = fun httpContent ->
                async {
                    let! bytes =
                        httpContent.ReadAsByteArrayAsync()
                        |> Async.AwaitTask

                    return ByteArrayContent bytes
                }

            let readAsString : MediaType -> HttpContent -> Async<Content> = fun mediaType httpContent ->
                async {
                    let! text =
                        httpContent.ReadAsStringAsync()
                        |> Async.AwaitTask

                    return StringContent (mediaType, text)
                }

            let toContent : HttpContent -> Async<Content> = fun httpContent ->
                match tryMediaType httpContent.Headers with
                | Some mediaType ->
                    if isByteArray mediaType
                    then readAsByteArray httpContent
                    else readAsString mediaType httpContent
                | None ->
                    readAsByteArray httpContent

            let tryReadResponseContent : HttpResponseMessage -> Async<Result<ResponseContent option, exn>> = fun httpResponseMessage ->
                async {
                    try
                        let httpContent = httpResponseMessage.Content

                        match getHeadersAndContentLength httpContent with
                        | [], 0L ->
                            return Ok None
                        | headers, length ->
                            let! data = toContent httpContent
                            let content = toResponseContent headers data
                            return Ok (Some content)
                    with ex ->
                        return Error ex
                }

    let toResponse : Request -> HttpResponseMessage -> ResponseContent option -> Response = fun request response responseContent -> {
            StatusCode = response.StatusCode
            Headers = Header.ofHttpHeaders response.Headers
            Content = responseContent
            Request = request
        }

    let isSuccessStatusCode : HttpStatusCode -> bool = fun code ->
        let code = int code
        200 <= code && code <= 299

    let ensureSuccessStatusCode : Response -> Result<Response, ResponseError> = fun response ->
        match response.StatusCode with
        | code when isSuccessStatusCode code ->
            Ok response
        | _ ->
            Error (NonSuccessHttpStatusCode response)

    let responseContentToResponse request (httpResponseMessage: HttpResponseMessage) =
        (toResponse request httpResponseMessage) >> ensureSuccessStatusCode

    module Task =

        let ofHttpResponseMessage : Request -> HttpResponseMessage -> Task<Result<Response, ResponseError>> = fun request httpResponseMessage ->
            Content.Task.tryReadResponseContent httpResponseMessage
            |> TaskResult.mapError (fun ex -> ResponseContentException (request, httpResponseMessage, ex))
            |> TaskResult.bind ((responseContentToResponse request httpResponseMessage) >> Task.retn)

        let parse<'a> : Map<MediaType, Deserializer<'a>> -> Response -> Task<Result<'a, ParseError>> = fun deserializers response ->
            match response.Content with
            | None ->
                let message = "Cannot parse response. The content is missing."
                backgroundTask { return Error (ParseError message) }
            | Some content ->
                match content.Content with
                | StringContent (mediaType, string) ->
                    HttpResponseContent.Task.parse<'a> deserializers (HttpContent.ofString mediaType string)
                | ByteArrayContent _ ->
                    let message = "Cannot parse response. The content is a byte array."
                    backgroundTask { return Error (ParseError message) }
                | FormUrlEncodedContent _ ->
                    let message = "Cannot parse response. The content is form URL encoded."
                    backgroundTask { return Error (ParseError message) }

    module Async =

        let ofHttpResponseMessage : Request -> HttpResponseMessage -> Async<Result<Response, ResponseError>> = fun request httpResponseMessage ->
            Content.Async.tryReadResponseContent httpResponseMessage
            |> AsyncResult.mapError (fun ex -> ResponseContentException (request, httpResponseMessage, ex))
            |> AsyncResult.bind ((responseContentToResponse request httpResponseMessage) >> Async.retn)

        let parse<'a> : Map<MediaType, Deserializer<'a>> -> Response -> Async<Result<'a, ParseError>> = fun deserializers response ->
            match response.Content with
            | None ->
                let message = "Cannot parse response. The content is missing."
                async { return Error (ParseError message) }
            | Some content ->
                match content.Content with
                | StringContent (mediaType, string) ->
                    HttpResponseContent.Async.parse<'a> deserializers (HttpContent.ofString mediaType string)
                | ByteArrayContent _ ->
                    let message = "Cannot parse response. The content is a byte array."
                    async { return Error (ParseError message) }
                | FormUrlEncodedContent _ ->
                    let message = "Cannot parse response. The content is form URL encoded."
                    async { return Error (ParseError message) }
