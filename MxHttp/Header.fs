namespace MxHttp

open System
open System.Text
open MxHttp

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

    let authorization (tokenType: string) (accessToken: string) =
        let key = Key "Authorization"
        let value = Value $"{tokenType} {accessToken}"
        key, [value]

    let basicAuthorization (username, password) =
        sprintf "%s:%s" username password
        |> Encoding.UTF8.GetBytes
        |> Convert.ToBase64String
        |> authorization "Basic"

    let bearerAuthorization accessToken =
        authorization "Bearer" accessToken

    let accept (MediaType mediaType) =
        let key = Key "Accept"
        let value = Value mediaType
        key, [value]

    let acceptJson =
        accept MediaType.applicationJson

    let acceptXml =
        accept MediaType.applicationXml
