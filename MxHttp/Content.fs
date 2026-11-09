namespace MxHttp

type Content =
| StringContent of MediaType * string
| ByteArrayContent of byte array
| FormUrlEncodedContent of (Key * Value) list

type Serializer<'a> = 'a -> string

module Content =

    let string serialize mediaType object =
        let content = serialize object
        StringContent (mediaType, content)

    let byteArray bytes =
        ByteArrayContent bytes

    let formUrlEncoded keyValuePairs =
        FormUrlEncodedContent keyValuePairs

    let toHttpContent = function
        | StringContent (mediaType, text) ->
            HttpContent.ofString mediaType text

        | ByteArrayContent bytes ->
            HttpContent.ofByteArray bytes

        | FormUrlEncodedContent keyValuePairs ->
            HttpContent.ofKeyValuePairs keyValuePairs
