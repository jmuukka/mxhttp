namespace MxHttp

type MediaType = private MediaType of string

module MediaType =

    let applicationJson = MediaType "application/json"

    let applicationXml = MediaType "application/xml"

    let applicationOctetStream = MediaType "application/octet-stream"

    let applicationFormUrlEncoded = MediaType "application/x-www-form-urlencoded"

    let ofString s = MediaType s

    let toString (MediaType mediaType) = mediaType
