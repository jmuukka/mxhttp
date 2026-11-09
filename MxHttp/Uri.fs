namespace MxHttp

open System

//module private Uri' =

//    let concat (left: Uri) (right: Uri) =
//        let left = left.ToString()
//        let right = right.ToString()

//        if left.EndsWith "/" then
//            if right.StartsWith "/" then
//                left + right.Substring(1)
//            else
//                left + right
//        else
//            if right.StartsWith "/" then
//                left + right
//            else
//                left + "/" + right

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

    let toString uri = (toSystemUri uri).ToString()

    //let concat (RelativeUri left) (RelativeUri right) =
    //    Uri'.concat left right
    //    |> ofString

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

    let toString uri = (toSystemUri uri).ToString()

    //let concat (AbsoluteUri left) (RelativeUri right) =
    //    Uri'.concat left right
    //    |> ofString

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
