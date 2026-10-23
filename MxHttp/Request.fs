namespace MxHttp

open System.Collections.Generic
open System.Net.Http
open System.Text

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
            let content = new ByteArrayContent(bytes) :> HttpContent
            let mediaType = MediaType.toString MediaType.applicationOctetStream
            content.Headers.ContentType <- Headers.MediaTypeHeaderValue(mediaType)
            content

        | FormUrlEncodedContent keyValuePairs ->
            let keyValuePairs = List.map (fun ((Key key), (Value value)) -> KeyValuePair<string, string>(key, value)) keyValuePairs
            new FormUrlEncodedContent(keyValuePairs) :> HttpContent

module HttpMethod =

    let patch = HttpMethod("PATCH")

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
    let ofHttpRequestMessage request =
        new HttpRequestMessage(request.Method, Uri.toSystemUri request.ResourceUri)

    // (Key * Value seq) list -> HttpRequestMessage -> HttpRequestMessage
    let applyHeaders headers (request: HttpRequestMessage) =
        let applyHeader header =
            let key = fst header
            let values = snd header
            request.Headers.Add(Key.toString key, Seq.map Value.toString values)

        List.iter applyHeader headers
        request

    // RequestContent option -> HttpRequestMessage -> HttpRequestMessage
    let applyContent content (request: HttpRequestMessage) =
        match content with
        | Some content ->
            request.Content <- RequestContent.toHttpContent content
        | None ->
            ()
        request

    // Request -> HttpRequestMessage
    let toHttpRequestMessage request =
        ofHttpRequestMessage request
        |> applyHeaders request.Headers
        |> applyContent request.Content
