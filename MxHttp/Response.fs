namespace MxHttp

open System.Collections.Generic
open System.Net
open System.Net.Http
open System.Net.Http.Headers
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

    // KeyValuePair<string, string seq> -> Key * Value list
    let toHeader (kvp: KeyValuePair<string, string seq>) =
        Key kvp.Key, Seq.map Value kvp.Value |> List.ofSeq

    // (KeyValuePair<string, string seq>) seq -> (Key * Value list) list
    let toHeaders headers =
        Seq.map toHeader headers
        |> List.ofSeq

    module Content =

        open System.Net.Http.Headers

        // HttpContentHeaders -> ResponseContentData -> ResponseContent
        let toResponseContent headers responseContentData = {
                Headers = headers
                Data = responseContentData
            }

        // HttpContentHeaders -> bool
        let isByteArray (headers: HttpContentHeaders) =
            let contentType = headers.ContentType
            if contentType = null then
                true
            else
                let mediaType = contentType.MediaType

                MediaType mediaType = MediaType.applicationOctetStream

        module Task =

            // HttpContent -> Task<ResponseContentData>
            let readAsByteArray (content: HttpContent) =
                backgroundTask {
                    let! bytes = content.ReadAsByteArrayAsync()

                    return ByteArrayResponseContent bytes
                }

            // HttpContent -> Task<ResponseContentData>
            let readAsString (content: HttpContent) =
                backgroundTask {
                    let! text = content.ReadAsStringAsync()

                    return StringResponseContent text
                }

            // HttpResponseMessage -> Task<ResponseContentData option>
            let toResponseContentData (content: HttpContent) =
                if isByteArray content.Headers
                then readAsByteArray content
                else readAsString content

            let tryReadResponseContent (httpResponseMessage: HttpResponseMessage) : Task<Result<ResponseContent option, exn>> =
                backgroundTask {
                    try
                        let httpContent = httpResponseMessage.Content

                        // Take the original headers.
                        let headers = toHeaders httpContent.Headers

                        // ContentLength property has a side-effect when the header value is missing.
                        // When it's read then it adds the Content-Length header value.
                        // This is the reason we will access the ContentLength property
                        // after copying the original header values.
                        let lengthOrNull = httpContent.Headers.ContentLength

                        match lengthOrNull.GetValueOrDefault(), headers with
                        | 0L, [] ->
                            return Ok None
                        | _ ->
                            let! data = toResponseContentData httpContent
                            let content = toResponseContent headers data
                            return Ok (Some content)
                    with ex ->
                        return Error ex
                }

        module Async =

            // HttpContent -> Async<ResponseContentData>
            //let readAsStream (content: HttpContent) =
            //    async {
            //        let! stream =
            //            content.ReadAsStreamAsync()
            //            |> Async.AwaitTask
            //        return StreamResponseContent stream
            //    }

            // HttpContent -> Async<ResponseContentData>
            let readAsByteArray (content: HttpContent) =
                async {
                    let! bytes =
                        content.ReadAsByteArrayAsync()
                        |> Async.AwaitTask

                    return ByteArrayResponseContent bytes
                }

            // HttpContent -> Async<ResponseContentData>
            let readAsString (content: HttpContent) =
                async {
                    let! text =
                        content.ReadAsStringAsync()
                        |> Async.AwaitTask

                    return StringResponseContent text
                }

            // HttpResponseMessage -> Async<ResponseContentData option>
            let toResponseContentData (content: HttpContent) =
                if isByteArray content.Headers
                then readAsByteArray content
                else readAsString content

            // HttpResponseMessage -> Async<Result<ResponseContent option, exn>>
            let tryReadResponseContent (httpResponseMessage: HttpResponseMessage) =
                async {
                    try
                        let httpContent = httpResponseMessage.Content

                        // Take the original headers.
                        let headers = toHeaders httpContent.Headers

                        // ContentLength property has a side-effect when the header value is missing.
                        // When it's read then it adds the Content-Length header value.
                        // This is the reason we will access the ContentLength property
                        // after copying the original header values.
                        let lengthOrNull = httpContent.Headers.ContentLength

                        match lengthOrNull.GetValueOrDefault(), headers with
                        | 0L, [] ->
                            return Ok None
                        | _ ->
                            let! data = toResponseContentData httpContent
                            let content = toResponseContent headers data
                            return Ok (Some content)
                    with ex ->
                        return Error ex
                }

    // Request -> HttpResponseMessage -> ResponseContent option -> Response
    let toResponse request (response: HttpResponseMessage) content = {
            StatusCode = response.StatusCode
            Headers = toHeaders response.Headers
            Content = content
            Request = request
        }

    // HttpStatusCode -> bool
    let isSuccessStatusCode (code: HttpStatusCode) =
        let code = int code
        200 <= code && code <= 299

    // Response -> Result<Response, Failure>
    let ensureSuccessStatusCode response =
        match response.StatusCode with
        | code when isSuccessStatusCode code ->
            Ok response
        | _ ->
            Error (NonSuccessHttpStatusCode response)

    let responseContentToResponse request (httpResponseMessage: HttpResponseMessage) =
        (toResponse request httpResponseMessage) >> ensureSuccessStatusCode

    module Task =

        // Request -> HttpResponseMessage -> Task<Result<Response, ResponseError>>
        let ofHttpResponseMessage request httpResponseMessage : Task<Result<Response, ResponseError>> =
            Content.Task.tryReadResponseContent httpResponseMessage
            |> TaskResult.mapError (fun ex -> ResponseContentException (request, httpResponseMessage, ex))
            |> TaskResult.bind ((responseContentToResponse request httpResponseMessage) >> Task.retn)

    module Async =
        
        // add support to read it as a stream

        // Request -> HttpResponseMessage -> Async<Result<Response, ResponseError>>
        let ofHttpResponseMessage request httpResponseMessage =
            Content.Async.tryReadResponseContent httpResponseMessage
            |> AsyncResult.mapError (fun ex -> ResponseContentException (request, httpResponseMessage, ex))
            |> AsyncResult.bind ((responseContentToResponse request httpResponseMessage) >> Async.retn)

    // Parse

    // ResponseContent -> string option
    let contentTypeOf (content: ResponseContent) =
        match List.tryFind (fun (Key key, _) -> key = "Content-Type") content.Headers with
        | Some (_, [mediaType]) -> mediaType |> Value.toString |> Some
        | _ -> None

    // ResponseContent -> string option
    let mediaTypeOf content =
        match contentTypeOf content with
        | Some contentType ->
            match MediaTypeHeaderValue.TryParse contentType with
            | true, value -> Some value.MediaType
            | false, _ -> None
        | None ->
            None

    // Map<MediaType, Deserializer<'a>> -> Response -> Result<'a, ResponseError>
    let parse<'a> (deserializers: Map<MediaType, Deserializer<'a>>) (response: Response) =
        match response.Content with
        | None ->
            let message = "There is no content."
            Error (ParseError (response, message))
        | Some content ->
            let mediaType =
                mediaTypeOf content
                |> Option.map MediaType

            match mediaType with
            | Some mediaType ->
                match Map.tryFind mediaType deserializers with
                | Some deserialize ->
                    match content.Data with
                    | StringResponseContent text ->
                        try
                            Ok (deserialize text)
                        with ex ->
                            Error (DeserializeException (response, ex))
                    | ByteArrayResponseContent _ ->
                        let message = "The content is a byte array. Cannot parse byte array to an object."
                        Error (ParseError (response, message))
                | None ->
                    let message = $"Deserializer for media type {MediaType.toString mediaType} is not configured."
                    Error (ParseError (response, message))
            | None ->
                let message = "Unknown media type."
                Error (ParseError (response, message))
