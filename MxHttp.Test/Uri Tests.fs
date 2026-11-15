module MxHttp.UriTests

open System
open Xunit

(******************** AbsoluteUri ********************)

[<Fact>]
let ``given a valid absolute URI when ofString then toString equals with an original string`` () =
    let uri = "http://localhost"

    let actual = AbsoluteUri.ofString uri

    Assert.Equal(uri, AbsoluteUri.toString actual)

[<Fact>]
let ``given a valid absolute URI when ofString then toSystemUri equals with an original string`` () =
    let uri = "http://localhost"
    let systemUri = Uri(uri, UriKind.Absolute)

    let actual = AbsoluteUri.ofString uri

    Assert.Equal(systemUri, AbsoluteUri.toSystemUri actual)

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
    | Some actual -> Assert.Equal(uri, AbsoluteUri.toString actual)
    | None -> failwith "AbsoluteUri.tryOfString should have succeeded for valid URI"

[<Fact>]
let ``given an invalid absolute URI when tryOfString then returns None`` () =
    let uri = "https//localhost"

    match AbsoluteUri.tryOfString uri with
    | Some u -> failwith "AbsoluteUri.tryOfString should not have succeeded for invalid URI"
    | None -> ()

[<Theory>]
[<InlineData("http://localhost", "api", "http://localhost/api")>]
[<InlineData("http://localhost/", "/api", "http://localhost/api")>]
[<InlineData("http://localhost/", "api", "http://localhost/api")>]
[<InlineData("http://localhost", "/api", "http://localhost/api")>]
[<InlineData("http://localhost", "/api?p=1", "http://localhost/api?p=1")>]
[<InlineData("http://localhost?p=1", "/api", "http://localhost/api?p=1")>]
[<InlineData("http://localhost?p1=1", "/api?p2=a", "http://localhost/api?p1=1&p2=a")>]
[<InlineData("http://localhost?p1=1", "/api?p1=a", "http://localhost/api?p1=1&p1=a")>]
[<InlineData("http://localhost?p1", "/api?p2", "http://localhost/api?p1&p2")>]
let ``given an absolute URI and a relative URI when concat then returns concatenated URI`` left right expected =
    let left = AbsoluteUri.ofString left
    let right = RelativeUri.ofString right

    let actual = AbsoluteUri.concat left right

    Assert.Equal(expected, AbsoluteUri.toString actual)

(******************** RelativeUri ********************)

[<Fact>]
let ``given a valid relative URI when ofString then toString equals with an original string`` () =
    let uri = "api/customers"

    let actual = RelativeUri.ofString uri

    Assert.Equal(uri, RelativeUri.toString actual)

[<Fact>]
let ``given a valid relative URI when ofString then toSystemUri equals with an original string`` () =
    let uri = "api/customers"
    let systemUri = Uri(uri, UriKind.Relative)

    let actual = RelativeUri.ofString uri

    Assert.Equal(systemUri, RelativeUri.toSystemUri actual)

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

[<Theory>]
[<InlineData("api", "customers", "api/customers")>]
[<InlineData("api/", "/customers", "api/customers")>]
[<InlineData("api/", "customers", "api/customers")>]
[<InlineData("api", "/customers", "api/customers")>]
[<InlineData("api", "/customers?p=1", "api/customers?p=1")>]
[<InlineData("api?p=1", "customers", "api/customers?p=1")>]
[<InlineData("api?p1=1", "/customers?p2=a", "api/customers?p1=1&p2=a")>]
[<InlineData("api?p1=1", "/customers?p1=a", "api/customers?p1=1&p1=a")>]
[<InlineData("api?p1", "/customers?p2", "api/customers?p1&p2")>]
let ``given two relative URIs when concat then returns concatenated URI`` left right expected =
    let left = RelativeUri.ofString left
    let right = RelativeUri.ofString right

    let actual = RelativeUri.concat left right

    Assert.Equal(expected, RelativeUri.toString actual)

(******************** Uri ********************)

[<Fact>]
let ``given an absolute URI string when absolute then returns a Uri with AbsoluteUri case`` () =
    let uri = "http://localhost"

    let actual = Uri.absolute uri

    Assert.Equal(Uri(uri, UriKind.Absolute), Uri.toSystemUri actual)

[<Fact>]
let ``given a relative URI string when relative then returns a Uri with RelativeUri case`` () =
    let uri = "api/customers"

    let actual = Uri.relative uri

    Assert.Equal(Uri(uri, UriKind.Relative), Uri.toSystemUri actual)
