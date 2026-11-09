namespace MxHttp

open System.Net.Http

type [<NoComparison>] Request = {
    Method: HttpMethod
    ResourceUri: Uri
    Headers: Header list
    Content: Content option
}

module Request =

    let get headers resourceUri =
        {
            Method = HttpMethod.Get
            ResourceUri = resourceUri
            Headers = headers
            Content = None
        }

    let query headers resourceUri content =
        {
            Method = HttpMethod.query
            ResourceUri = resourceUri
            Headers = headers
            Content = Some content
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

    let replaceContent : Content option -> HttpRequestMessage -> HttpRequestMessage = fun content request ->
        match content with
        | Some content ->
            request.Content <- Content.toHttpContent content
        | None ->
            ()
        request

    let toHttpRequestMessage : Request -> HttpRequestMessage = fun request ->
        HttpRequestMessage.create request.Method request.ResourceUri
        |> HttpRequestMessage.applyHeaders request.Headers
        |> replaceContent request.Content
