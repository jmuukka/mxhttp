namespace MxHttp

open System.IO
open System.Net
open System.Net.Http
open System.Threading
open System.Threading.Tasks

/// Deserialize a string to an object of type 'a.
type Deserializer<'a> = string -> 'a

[<NoComparison>]
type ParseError =
| ParseError of string
| DeserializeException of exn

/// Functions to access HttpContent of HttpResponseMessage.
module HttpResponseContent =

    module Task =

        val readAsStream: HttpContent -> Task<Stream>

        val readAsByteArray: HttpContent -> Task<byte array>

        val readAsString: HttpContent -> Task<string>

        val parse<'t> : Map<MediaType, Deserializer<'t>> -> HttpContent -> Task<Result<'t, ParseError>>

    module Async =

        val readAsStream: HttpContent -> Async<Stream>

        val readAsByteArray: HttpContent -> Async<byte array>

        val readAsString: HttpContent -> Async<string>

        val parse<'t> : Map<MediaType, Deserializer<'t>> -> HttpContent -> Async<Result<'t, ParseError>>
