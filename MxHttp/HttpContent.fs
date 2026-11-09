namespace MxHttp

open System.Collections.Generic
open System.Net.Http
open System.Net.Http.Headers
open System.Text

module HttpContent =

    let ofString mediaType text =
        new StringContent(text, Encoding.UTF8, MediaType.toString mediaType) :> HttpContent

    let ofByteArray bytes =
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

    let applyHeaders headers (httpContent: HttpContent) =
        HttpHeader.applyHeaders headers httpContent.Headers
        httpContent
