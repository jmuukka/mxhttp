namespace MxHttp

open System.IO
open System.Net.Http

module HttpContent =

    /// Construct an HttpContent of a string.
    val ofString: MediaType -> string -> HttpContent

    /// Construct an HttpContent of a byte array.
    val ofByteArray: byte array -> HttpContent

    /// Construct a Form URL encoded HttpContent of a list of key and value pairs.
    val ofKeyValuePairs: (Key * Value) list -> HttpContent

    /// Construct an HttpContent of a stream.
    val ofStream: MediaType -> Stream -> HttpContent

    /// Apply the headers to the HttpContent.
    /// When the HttpContent already has the Key then replaces the values.
    /// Updates the HttpContent object.
    val applyHeaders: Header list -> HttpContent -> HttpContent
