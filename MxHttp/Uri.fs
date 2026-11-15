namespace MxHttp

open System

module private Uri' =

    type QueryPart =
    | WithValue of string * string
    | WithoutValue of string

    module QueryPart =

        let ofString : string -> QueryPart = fun kvp ->
            let index = kvp.IndexOf('=')
            if index >= 0 then
                WithValue (kvp.Substring(0, index), kvp.Substring(index + 1))
            else
                WithoutValue kvp

        let toString : QueryPart -> string = function
            | WithValue (key, value) -> key + "=" + value
            | WithoutValue key -> key

    let parseQueryString : string -> QueryPart array = fun queryString ->
        queryString.Split([|'&'|], StringSplitOptions.RemoveEmptyEntries)
        |> Array.map QueryPart.ofString

    let concatQueryStrings : QueryPart array -> QueryPart array -> string = fun leftQueryStrings rightQueryStrings ->
        Array.append leftQueryStrings rightQueryStrings
        |> Array.map QueryPart.toString
        |> String.concat "&"

    let concatUriStrings : string -> string -> string = fun leftPath rightPath ->
        if leftPath.EndsWith "/" then
            if rightPath.StartsWith "/" then
                leftPath + rightPath.Substring(1)
            else
                leftPath + rightPath
        else
            if rightPath.StartsWith "/" then
                leftPath + rightPath
            else
                leftPath + "/" + rightPath

    let splitIntoPathAndQuery : Uri -> string * string = fun uri ->
        let rightPath = uri.OriginalString
        let index = rightPath.IndexOf('?')
        if index >= 0 then
            rightPath.Substring(0, index), rightPath.Substring(index + 1)
        else
            rightPath, ""

    let concatPathAndQuery : string -> string -> string = fun path query ->
        if String.length query = 0 then
            path
        else
            path + "?" + query

    let concat : Uri -> Uri -> string = fun absoluteOrRelativeUri relativeUri ->
        let leftPath, leftQuery = splitIntoPathAndQuery absoluteOrRelativeUri
        let rightPath, rightQuery = splitIntoPathAndQuery relativeUri

        let path = concatUriStrings leftPath rightPath

        let leftQueryStrings = parseQueryString leftQuery
        let rightQueryStrings = parseQueryString rightQuery
        let query = concatQueryStrings leftQueryStrings rightQueryStrings

        concatPathAndQuery path query

type [<NoComparison>] RelativeUri = private RelativeUri of Uri

module RelativeUri =

    let ofString uri =
        Uri(uri, UriKind.Relative)
        |> RelativeUri

    let tryOfString uri =
        try
            ofString uri
            |> Some
        with _ ->
            None

    let toSystemUri (RelativeUri uri) = uri

    let toString uri = (toSystemUri uri).OriginalString

    let concat (RelativeUri left) (RelativeUri right) =
        Uri'.concat left right
        |> ofString

type [<NoComparison>] AbsoluteUri = private AbsoluteUri of Uri

module AbsoluteUri =

    let ofString uri =
        Uri(uri, UriKind.Absolute)
        |> AbsoluteUri

    let tryOfString uri =
        try
            ofString uri
            |> Some
        with _ ->
            None

    let toSystemUri (AbsoluteUri uri) = uri

    let toString uri = (toSystemUri uri).OriginalString

    let concat (AbsoluteUri left) (RelativeUri right) =
        Uri'.concat left right
        |> ofString

[<NoComparison>]
[<RequireQualifiedAccess>]
type Uri =
| Absolute of AbsoluteUri
| Relative of RelativeUri

module Uri =

    let absolute uri =
        AbsoluteUri.ofString uri
        |> Uri.Absolute

    let relative uri =
        RelativeUri.ofString uri
        |> Uri.Relative

    let toSystemUri = function
        | Uri.Absolute uri -> AbsoluteUri.toSystemUri uri
        | Uri.Relative uri -> RelativeUri.toSystemUri uri
