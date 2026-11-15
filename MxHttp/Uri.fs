namespace MxHttp

open System

module private Uri' =

    type QueryPart =
    | WithValue of string * string
    | WithoutValue of string

    module QueryPart =

        let ofString (kvp: string) =
            let index = kvp.IndexOf('=')
            if index >= 0 then
                WithValue (kvp.Substring(0, index), kvp.Substring(index + 1))
            else
                WithoutValue kvp

        let toString = function
            | WithValue (key, value) -> key + "=" + value
            | WithoutValue key -> key

    let parseQueryString : string -> QueryPart array = fun queryString ->
        let queryString = if queryString.StartsWith "?" then queryString.Substring(1) else queryString

        queryString.Split([|'&'|], StringSplitOptions.RemoveEmptyEntries)
        |> Array.map QueryPart.ofString

    let concatQueryStrings leftQueryStrings rightQueryStrings =
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

    let concat : Uri -> Uri -> string = fun absoluteUri relativeUri ->
        let leftPath = absoluteUri.GetLeftPart(UriPartial.Path)
        let leftQueryStrings = parseQueryString absoluteUri.Query
        let rightPath = relativeUri.OriginalString
        let rightPath, rightQuery =
            let rightPath = relativeUri.OriginalString
            let index = rightPath.IndexOf('?')
            if index >= 0 then
                rightPath.Substring(0, index), rightPath.Substring(index + 1)
            else
                rightPath, ""
        let rightQueryStrings = parseQueryString rightQuery
        let queryStrings = concatQueryStrings leftQueryStrings rightQueryStrings

        let queryString =
            if queryStrings.Length > 0 then
                "?" + queryStrings
            else
                queryStrings

        (concatUriStrings leftPath rightPath) + queryString

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
