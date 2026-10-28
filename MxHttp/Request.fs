namespace MxHttp

open System.Collections.Generic
open System.Net.Http
open System.Net.Http.Headers
open System.Text

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

    let patch = HttpMethod("PATCH")
    
    //let query = HttpMethod("QUERY")

module RequestContent =

    let string serialize mediaType object =
        let content = serialize object
        StringContent (mediaType, content)

    let byteArray bytes =
        ByteArrayContent bytes

    let formUrlEncoded keyValuePairs =
        List.map (fun (key: string, value: string) -> Key key, Value value) keyValuePairs
        |> FormUrlEncodedContent

module HttpContent =

    let ofString mediaType text =
        new StringContent(text, Encoding.UTF8, MediaType.toString mediaType) :> HttpContent

    let ofBytes bytes =
        let content = new ByteArrayContent(bytes)
        let mediaType = MediaType.toString MediaType.applicationOctetStream
        content.Headers.ContentType <- MediaTypeHeaderValue(mediaType)
        content :> HttpContent

    let ofKeyValuePairs keyValuePairs =
        let keyValuePairs = List.map (fun ((Key key), (Value value)) -> KeyValuePair<string, string>(key, value)) keyValuePairs
        new FormUrlEncodedContent(keyValuePairs) :> HttpContent

    let ofStream mediaType stream =
        let content = new StreamContent(stream)
        content.Headers.ContentType <- MediaTypeHeaderValue(MediaType.toString mediaType)
        content :> HttpContent

    let ofRequestContent = function
        | StringContent (mediaType, text) ->
            ofString mediaType text

        | ByteArrayContent bytes ->
            ofBytes bytes

        | FormUrlEncodedContent keyValuePairs ->
            ofKeyValuePairs keyValuePairs

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

module HttpRequestMessage =

    // Request -> HttpRequestMessage
    let ofHttpRequestMessage httpMethod resourceUri =
        new HttpRequestMessage(httpMethod, Uri.toSystemUri resourceUri)

    // (Key * Value seq) list -> HttpRequestMessage -> HttpRequestMessage
    let applyHeaders headers (request: HttpRequestMessage) =
        let applyHeader header =
            let key = fst header
            let values = snd header
            request.Headers.Add(Key.toString key, Seq.map Value.toString values)

        List.iter applyHeader headers
        request

    // HttpContent -> HttpRequestMessage -> HttpRequestMessage
    let applyHttpContent httpContent (request: HttpRequestMessage) =
        request.Content <- httpContent
        request

    // RequestContent option -> HttpRequestMessage -> HttpRequestMessage
    let applyContent content (request: HttpRequestMessage) =
        match content with
        | Some content ->
            request.Content <- HttpContent.ofRequestContent content
        | None ->
            ()
        request

    let ofHttpContent httpMethod (headers: Header list) resourceUri httpContent =
        ofHttpRequestMessage httpMethod resourceUri
        |> applyHeaders headers
        |> applyHttpContent httpContent

    let ofRequest request =
        ofHttpRequestMessage request.Method request.ResourceUri
        |> applyHeaders request.Headers
        |> applyContent request.Content
