namespace MxHttp

open System.IO
open System.Net.Http
open System.Threading
open System.Threading.Tasks

type Deserializer<'a> = string -> 'a

[<NoComparison>]
type ParseError =
| ParseError of HttpContent * string
| DeserializeException of HttpContent * exn

module HttpResponseContent =

    let mediaTypeOf : HttpContent -> string option = fun httpContent ->
        let contentType = httpContent.Headers.ContentType
        if contentType = null then
            None
        else
            Some contentType.MediaType

    let tryResolveDeserializer : Map<MediaType, Deserializer<'a>> -> HttpContent -> Result<Deserializer<'a>, ParseError> = fun deserializers httpContent ->
        match mediaTypeOf httpContent with
        | Some mediaType ->
            let deserializerWithSameMediaType : MediaType -> Deserializer<'a> -> Deserializer<'a> option = fun key value ->
                match System.String.Equals(MediaType.toString key, mediaType, System.StringComparison.OrdinalIgnoreCase) with
                | true -> Some value
                | false -> None

            match Map.tryPick deserializerWithSameMediaType deserializers with
            | Some deserializer ->
                Ok deserializer
            | None ->
                let message = $"Deserializer for media type {mediaType} is not configured."
                Error (ParseError (httpContent, message))
        | None ->
            let message = "The HTTP response content does not have a media type."
            Error (ParseError (httpContent, message))

    let tryDeserialize : Deserializer<'a> -> string -> Result<'a, exn> = fun deserialize string ->
        try
            Ok (deserialize string)
        with ex ->
            Error ex

    module Task =

        let readAsStream : HttpContent -> Task<Stream> = fun httpContent ->
            httpContent.ReadAsStreamAsync()

        let readAsByteArray : HttpContent -> Task<byte array> = fun httpContent ->
            httpContent.ReadAsByteArrayAsync()

        let readAsString : HttpContent -> Task<string> = fun httpContent ->
            httpContent.ReadAsStringAsync()

        let parse<'a> : Map<MediaType, Deserializer<'a>> -> HttpContent -> Task<Result<'a, ParseError>> = fun deserializers httpContent ->
            backgroundTask {
                match tryResolveDeserializer deserializers httpContent with
                | Ok deserializer ->
                    let! string = readAsString httpContent
                    return tryDeserialize deserializer string
                           |> Result.mapError (fun ex -> DeserializeException (httpContent, ex))
                | Error parseError ->
                    return Error parseError
            }

    module Async =

        let readAsStream : HttpContent -> Async<Stream> = fun httpContent ->
            async {
                let! stream =
                    httpContent.ReadAsStreamAsync()
                    |> Async.AwaitTask
                return stream
            }

        let readAsByteArray : HttpContent -> Async<byte array> = fun httpContent ->
            async {
                let! bytes =
                    httpContent.ReadAsByteArrayAsync()
                    |> Async.AwaitTask
                return bytes
            }

        let readAsString : HttpContent -> Async<string> = fun httpContent ->
            async {
                let! string =
                    httpContent.ReadAsStringAsync()
                    |> Async.AwaitTask
                return string
            }

        let parse<'a> : Map<MediaType, Deserializer<'a>> -> HttpContent -> Async<Result<'a, ParseError>> = fun deserializers httpContent ->
            async {
                match tryResolveDeserializer deserializers httpContent with
                | Ok deserializer ->
                    let! string = readAsString httpContent
                    return tryDeserialize deserializer string
                           |> Result.mapError (fun ex -> DeserializeException (httpContent, ex))
                | Error parseError ->
                    return Error parseError
            }
