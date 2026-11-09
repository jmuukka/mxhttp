namespace MxHttp

open System
open System.Text
open MxHttp
open System.Collections.Generic

type Key = private Key of string
type Value = private Value of string
type Header = Key * Value list

module Key =

    let ofString s = Key s

    let toString (Key key) = key

module Value =

    let ofString s = Value s

    let toString (Value value) = value

module Header =

    let multipleValues : string -> string list -> Header = fun key values ->
        let key = Key key
        key, List.map Value values

    let singleValue : string -> string -> Header = fun key value ->
        multipleValues key [value]

    let authorization : string -> string -> Header = fun tokenType accessToken ->
        let value = $"{tokenType} {accessToken}"
        singleValue "Authorization" value

    let basicAuthorization (username, password) =
        sprintf "%s:%s" username password
        |> Encoding.UTF8.GetBytes
        |> Convert.ToBase64String
        |> authorization "Basic"

    let bearerAuthorization accessToken =
        authorization "Bearer" accessToken

    let accept (MediaType mediaType) =
        singleValue "Accept" mediaType

    let acceptJson =
        accept MediaType.applicationJson

    let acceptXml =
        accept MediaType.applicationXml

    let private toHeader : KeyValuePair<string, string seq> -> Header = fun kvp ->
        Key kvp.Key, Seq.map Value kvp.Value |> List.ofSeq

    let ofHttpHeaders : KeyValuePair<string, string seq> seq -> Header list = fun headers ->
        Seq.map toHeader headers
        |> List.ofSeq
