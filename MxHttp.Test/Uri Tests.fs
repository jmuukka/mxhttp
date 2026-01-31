module MxHttp.UriTests

open System
open Xunit

[<Fact>]
let ``given a valid relative URI when ofString then toString equals with an original string`` () =
    let uri = "api/customers"

    let actual = RelativeUri.ofString uri

    Assert.Equal(uri, RelativeUri.toString actual)

[<Fact>]
let ``given an invalid relative URI when ofString then throws an exception`` () =
    let uri = "http://localhost"

    let created =
        try
            RelativeUri.ofString uri |> ignore
            true
        with _ ->
            false

    if created then failwith "RelativeUri.ofString should have thrown an exception for invalid URI"

[<Fact>]
let ``given a valid relative URI when tryOfString then returns Some relative URI and when converted toString then equals with an original string`` () =
    let uri = "api/customers"

    match RelativeUri.tryOfString uri with
    | Some actual -> Assert.Equal(uri, RelativeUri.toString actual)
    | None -> failwith "RelativeUri.tryOfString should have succeeded for valid URI"

[<Fact>]
let ``given an invalid relative URI when tryOfString then returns None`` () =
    let uri = "https://localhost"

    match RelativeUri.tryOfString uri with
    | Some _ -> failwith "RelativeUri.tryOfString should not have succeeded for invalid URI"
    | None -> ()

[<Fact>]
let ``given a valid absolute URI when ofString then toString equals with an original string`` () =
    let uri = "http://localhost"

    let actual = AbsoluteUri.ofString uri

    Assert.Equal(uri + "/", AbsoluteUri.toString actual)

[<Fact>]
let ``given an invalid absolute URI when ofString then throws an exception`` () =
    let uri = "http://local host"

    let created =
        try
            AbsoluteUri.ofString uri |> ignore
            true
        with _ ->
            false

    if created then failwith "AbsoluteUri.ofString should have thrown an exception for invalid URI"

[<Fact>]
let ``given a valid absolute URI when tryOfString then returns Some absolute URI and when converted toString then equals with an original string`` () =
    let uri = "http://localhost"

    match AbsoluteUri.tryOfString uri with
    | Some actual -> Assert.Equal(uri + "/", AbsoluteUri.toString actual)
    | None -> failwith "AbsoluteUri.tryOfString should have succeeded for valid URI"

[<Fact>]
let ``given an invalid absolute URI when tryOfString then returns None`` () =
    let uri = "https//localhost"

    match AbsoluteUri.tryOfString uri with
    | Some u -> failwith "AbsoluteUri.tryOfString should not have succeeded for invalid URI"
    | None -> ()

[<Theory>]
[<InlineData("api", "customers", "api/customers")>]
[<InlineData("api/", "/customers", "api/customers")>]
[<InlineData("api/", "customers", "api/customers")>]
[<InlineData("api", "/customers", "api/customers")>]
let ``given two relative URIs when concat then returns concatenated URI`` left right expected =
    let left = RelativeUri.ofString left
    let right = RelativeUri.ofString right

    let actual = RelativeUri.concat left right

    Assert.Equal(expected, RelativeUri.toString actual)

[<Theory>]
[<InlineData("http://localhost", "api", "http://localhost/api")>]
[<InlineData("http://localhost/", "/api", "http://localhost/api")>]
[<InlineData("http://localhost/", "api", "http://localhost/api")>]
[<InlineData("http://localhost", "/api", "http://localhost/api")>]
let ``given an absolute URI and a relative URI when concat then returns concatenated URI`` left right expected =
    let left = AbsoluteUri.ofString left
    let right = RelativeUri.ofString right

    let actual = AbsoluteUri.concat left right

    Assert.Equal(expected, AbsoluteUri.toString actual)

[<Fact>]
let ``given an absolute URI string when absolute then returns a Uri with AbsoluteUri case`` () =
    let uri = "http://localhost"

    let actual = Uri.absolute uri

    Assert.Equal(Uri(uri, UriKind.Absolute), Uri.systemUriOf actual)

[<Fact>]
let ``given a relative URI string when relative then returns a Uri with RelativeUri case`` () =
    let uri = "api/customers"

    let actual = Uri.relative uri

    Assert.Equal(Uri(uri, UriKind.Relative), Uri.systemUriOf actual)
