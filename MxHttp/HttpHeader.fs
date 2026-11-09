namespace MxHttp

open MxHttp
open System.Net.Http.Headers

module HttpHeader =

    /// Applies the headers in the HttpHeaders with the given headers.
    /// If a header with the same key already exists in the HttpHeaders, it will be replaced with the new values.
    let applyHeaders : Header list -> HttpHeaders -> unit = fun headers httpHeaders ->
        let replaceValues ((Key key), values) =
            let values = List.map Value.toString values

            match httpHeaders.TryGetValues(key) with
            | true, values ->
                httpHeaders.Remove(key)
                |> ignore
            | false, _ ->
                ()

            httpHeaders.Add(key, values)

        List.iter replaceValues headers
