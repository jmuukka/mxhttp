namespace MxHttp

open System
open System.Collections.Generic
open System.Net
open System.Net.Http
open System.Text
open System.Threading
open System.Threading.Tasks

[<Measure>]
type ms

module private Result =

    let either onOk onError = function
        | Ok value -> onOk value
        | Error error -> onError error

    let retn value = Ok value

module private Async =

    let bind binder input = async.Bind(input, binder)

    let inline retn value = async.Return value

module private AsyncResult =

    let error error =
        Error error
        |> Async.retn

    let bind binder input =
        Async.bind (Result.either binder error) input

[<AutoOpen>]
module private AsyncResultBuilder =

    type AsyncResultBuilder () =

        member _.Bind(asyncResult, binder) =
            AsyncResult.bind binder asyncResult

        member _.Return value =
            value
            |> Result.retn
            |> Async.retn

    let asyncResult = new AsyncResultBuilder()

type MediaType = private MediaType of string

module MediaType =

    let applicationJson = MediaType "application/json"

    let applicationXml = MediaType "application/xml"

    let applicationOctetStream = MediaType "application/octet-stream"

    let ofString s = MediaType s

    let toString (MediaType mediaType) = mediaType

type [<NoComparison>] AbsoluteUri = private AbsoluteUri of Uri
type [<NoComparison>] RelativeUri = private RelativeUri of Uri

module private Uri' =

    let concat (left: Uri) (right: Uri) =
        let left = left.ToString()
        let right = right.ToString()

        if left.EndsWith "/" then
            if right.StartsWith "/" then
                left + right.Substring(1)
            else
                left + right
        else
            if right.StartsWith "/" then
                left + right
            else
                left + "/" + right

module RelativeUri =

    let ofString uri =
        Uri(uri, UriKind.Relative)
        |> RelativeUri

    let tryOfString uri =
        try
            ofString uri
            |> Some
        with _ ->
            None

    let systemUriOf (RelativeUri uri) = uri

    let toString uri = (systemUriOf uri).ToString()

    let concat (RelativeUri left) (RelativeUri right) =
        Uri'.concat left right
        |> ofString

module AbsoluteUri =

    let ofString uri =
        Uri(uri, UriKind.Absolute)
        |> AbsoluteUri

    let tryOfString uri =
        try
            ofString uri
            |> Some
        with _ ->
            None

    let systemUriOf (AbsoluteUri uri) = uri

    let toString uri = (systemUriOf uri).ToString()

    let concat (AbsoluteUri left) (RelativeUri right) =
        Uri'.concat left right
        |> ofString

[<NoComparison>] 
[<RequireQualifiedAccess>]
type Uri =
| Absolute of AbsoluteUri
| Relative of RelativeUri

module Uri =

    let absolute uri =
        AbsoluteUri.ofString uri
        |> Uri.Absolute

    let relative uri =
        RelativeUri.ofString uri
        |> Uri.Relative

    let systemUriOf = function
        | Uri.Absolute uri -> AbsoluteUri.systemUriOf uri
        | Uri.Relative uri -> RelativeUri.systemUriOf uri

type Key = private Key of string
type Value = private Value of string
type Header = Key * Value list

type Deserializer<'a> = MediaType * (string -> 'a)

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

[<NoComparison>]
type Failure =
| RequestException of Request * exn
| ResponseContentException of Request * HttpResponseMessage * exn
| NonSuccessHttpStatusCode of Response
| ParseError of Response * string
| DeserializeException of Response * exn
| Failures of Failure list

type HttpSendRequest = CancellationToken -> HttpRequestMessage -> Task<HttpResponseMessage>

module Key =

    let ofString s = Key s

    let toString (Key key) = key

module Value =

    let ofString s = Value s

    let toString (Value value) = value

module Header =

    let authorization (tokenType: string) (accessToken: string) =
        let key = Key "Authorization"
        let value = Value $"{tokenType} {accessToken}"
        key, [value]

    let basicAuthorization (username, password) =
        sprintf "%s:%s" username password
        |> Encoding.UTF8.GetBytes
        |> Convert.ToBase64String
        |> authorization "Basic"

    let bearerAuthorization =
        authorization "Bearer"

    let accept (MediaType mediaType) =
        let key = Key "Accept"
        let value = Value mediaType
        key, [value]

    let acceptJson =
        accept MediaType.applicationJson

    let acceptXml =
        accept MediaType.applicationXml

module HttpMethod =

    let patch = HttpMethod("PATCH")

module RequestContent =

    // ('a -> string) -> MediaType -> 'a -> RequestContent
    let string serialize mediaType object =
        let content = serialize object
        StringContent (mediaType, content)

    // byte array -> RequestContent
    let byteArray bytes =
        ByteArrayContent bytes

    // (string * string) list -> RequestContent
    let formUrlEncoded keyValuePairs =
        List.map (fun (key: string, value: string) -> Key key, Value value) keyValuePairs
        |> FormUrlEncodedContent

    // RequestContent -> HttpContent
    let toHttpContent = function
        | StringContent ((MediaType mediaType), text) ->
            new StringContent(text, Encoding.UTF8, mediaType) :> HttpContent

        | ByteArrayContent bytes ->
            new ByteArrayContent(bytes) :> HttpContent

        | FormUrlEncodedContent keyValuePairs ->
            let keyValuePairs = List.map (fun ((Key key), (Value value)) -> KeyValuePair<string, string>(key, value)) keyValuePairs
            new FormUrlEncodedContent(keyValuePairs) :> HttpContent

module Request =

    let get headers resourceUri =
        {
            Method = HttpMethod.Get
            ResourceUri = resourceUri
            Headers = headers
            Content = None
        }

    let post headers resourceUri content =
        {
            Method = HttpMethod.Post
            ResourceUri = resourceUri
            Headers = headers
            Content = Some content
        }

    let put headers resourceUri content =
        {
            Method = HttpMethod.Put
            ResourceUri = resourceUri
            Headers = headers
            Content = Some content
        }

    let patch headers resourceUri content =
        {
            Method = HttpMethod.patch
            ResourceUri = resourceUri
            Headers = headers
            Content = Some content
        }

    let delete headers resourceUri =
        {
            Method = HttpMethod.Delete
            ResourceUri = resourceUri
            Headers = headers
            Content = None
        }

    // ---------------

    // Request -> HttpRequestMessage
    let private createHttpRequestMessage request =
        new HttpRequestMessage(request.Method, Uri.systemUriOf request.ResourceUri)

    // (Key * Value seq) list -> HttpRequestMessage -> HttpRequestMessage
    let private applyHeaders headers (request: HttpRequestMessage) =
        let applyHeader header =
            let key = fst header
            let values = snd header
            request.Headers.Add(Key.toString key, Seq.map Value.toString values)

        List.iter applyHeader headers
        request

    // RequestContent option -> HttpRequestMessage -> HttpRequestMessage
    let private applyContent content (request: HttpRequestMessage) =
        match content with
        | Some content ->
            request.Content <- RequestContent.toHttpContent content
        | None ->
            ()
        request

    // Request -> HttpRequestMessage
    let toHttpRequestMessage request =
        createHttpRequestMessage request
        |> applyHeaders request.Headers
        |> applyContent request.Content

module Response =

    // KeyValuePair<string, string seq> -> Key * Value list
    let private toHeader (kvp: KeyValuePair<string, string seq>) =
        Key kvp.Key, Seq.map Value kvp.Value |> List.ofSeq

    // (KeyValuePair<string, string seq>) seq -> (Key * Value list) list
    let private toHeaders headers =
        Seq.map toHeader headers
        |> List.ofSeq

    module internal Content =

        open System.Net.Http.Headers

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

        // HttpContentHeaders -> bool
        let isByteArray (headers: HttpContentHeaders) =
            if headers.ContentType :> obj = null
            then true
            else
                let mediaType = headers.ContentType.MediaType

                if String.IsNullOrWhiteSpace(mediaType)
                then true
                else MediaType mediaType = MediaType.applicationOctetStream

        // HttpResponseMessage -> Async<ResponseContentData option>
        let toResponseContentData (content: HttpContent) =
            if isByteArray content.Headers
            then readAsByteArray content
            else readAsString content

        // HttpContentHeaders -> ResponseContentData -> ResponseContent
        let toResponseContent (headers: HttpContentHeaders) responseContentData = {
                Headers = toHeaders headers
                Data = responseContentData
            }

        let tryReadResponseContentData request (httpResponseMessage: HttpResponseMessage) : Async<Result<ResponseContentData option, Failure>> =
            async {
                try
                    let httpContent = httpResponseMessage.Content
                    let contentLength = httpContent.Headers.ContentLength

                    let! contentDataMaybe =
                        if contentLength.GetValueOrDefault() = 0L then
                            async { return None }
                        else
                            async {
                                let! responseContentData = toResponseContentData httpContent
                                return Some responseContentData
                            }                            

                    return Ok contentDataMaybe
                with ex ->
                    return
                        (request, httpResponseMessage, ex)
                        |> ResponseContentException
                        |> Error
            }

    // Request -> HttpResponseMessage -> ResponseContent option -> Response
    let internal toResponse request (response: HttpResponseMessage) content = {
            StatusCode = response.StatusCode
            Headers = toHeaders response.Headers
            Content = content
            Request = request
        }

    // HttpStatusCode -> bool
    let private isSuccessStatusCode (code: HttpStatusCode) =
        let code = int code
        200 <= code && code <= 299

    // Response -> Result<Response, Failure>
    let internal ensureSuccessStatusCode response =
        match response.StatusCode with
        | code when isSuccessStatusCode code ->
            Ok response
        | _ ->
            Error (NonSuccessHttpStatusCode response)

    // Request -> HttpResponseMessage -> Async<Result<Response, Failure>>
    let ofHttpResponseMessage (request: Request) (httpResponseMessage: HttpResponseMessage) : Async<Result<Response, Failure>> =
        asyncResult {
            let! responseContentDataMaybe = Content.tryReadResponseContentData request httpResponseMessage
            let responseContentMaybe = Option.map (Content.toResponseContent httpResponseMessage.Content.Headers) responseContentDataMaybe
            let! response =
                toResponse request httpResponseMessage responseContentMaybe
                |> ensureSuccessStatusCode
                |> Async.retn
            return response
        }

    // Parse

    // Picks the first content type if more than one! Can there be more than one content type?
    // ResponseContent -> string option
    let private contentType (content: ResponseContent) =
        match List.tryFind (fun (Key key, _) -> key = "Content-Type") content.Headers with
        | Some (_, []) -> None
        | Some (_, mediaTypes) -> List.head mediaTypes |> Value.toString |> Some
        | None -> None

    // ResponseContent -> string option
    let private mediaType content =
        match contentType content with
        | Some contentType ->
            let parts = contentType.Split([|';'|])
            if parts.Length >= 1 && parts.[0].Length > 0 then Some parts.[0] else None
        | None ->
            None

    // Deserializer<'a> list -> Response -> Result<'a, Failure>
    let parse<'a> (deserializers: Deserializer<'a> list) (response: Response) =
        match response.Content with
        | None ->
            let message = "There is no content."
            Error (ParseError (response, message))
        | Some content ->
            let mediaType =
                mediaType content
                |> Option.defaultValue ""

            let equalsDeserializer deserializerData =
                (fst deserializerData) = MediaType mediaType

            match List.tryFind equalsDeserializer deserializers with
            | Some value ->
                match content.Data with
                | StringResponseContent text ->
                    let deserialize = snd value
                    try
                        Ok (deserialize text)
                    with ex ->
                        Error (DeserializeException (response, ex))
                | ByteArrayResponseContent _ ->
                    let message = "The content is a byte array. Cannot parse it to object."
                    Error (ParseError (response, message))
            | None ->
                let message = $"Deserializer for {mediaType} is not configured."
                Error (ParseError (response, message))

module Http =

    module Async =

        let private sendRequest send request =
            async {
                try
                    let! cancellationToken = Async.CancellationToken
                    let! httpResponseMessage =
                        request
                        |> Request.toHttpRequestMessage
                        |> send cancellationToken
                        |> Async.AwaitTask

                    return Ok httpResponseMessage
                with ex ->
                    return
                        (request, ex)
                        |> RequestException
                        |> Error
            }

        let send send request =
            asyncResult {
                let! httpResponseMessage = sendRequest send request
                let! response = Response.ofHttpResponseMessage request httpResponseMessage
                return response
            }

    module Sync =

        let send send timeout cancellationToken request =
            let computation = Async.send send request

            Async.RunSynchronously (computation, timeout / 1<ms>, cancellationToken)
