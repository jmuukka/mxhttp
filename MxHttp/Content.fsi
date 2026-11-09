namespace MxHttp

open System.Net.Http

/// The content.
type Content =
/// The string content with a media type.
| StringContent of MediaType * string
/// A byte array content.
| ByteArrayContent of byte array
/// A key and value pair content.
| FormUrlEncodedContent of (Key * Value) list

/// Serialize an object to a string.
type Serializer<'a> = 'a -> string

module Content =

    /// Construct a string content of an object of type 'a.
    val string: Serializer<'a> -> MediaType -> 'a -> Content

    /// Construct a byte array content.
    val byteArray: byte array -> Content

    /// Construct a form URL encoded content.
    val formUrlEncoded: (Key * Value) list -> Content

    /// Construct an HttpContent from a content.
    val toHttpContent: Content -> HttpContent
