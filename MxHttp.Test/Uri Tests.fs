module MxHttp.UriTests

open System
open Xunit

[<Fact>]
let ``given a valid relative URI when create then systemUri of it is an relative URI`` () =
    let uri = "api/customers"

    let actual = RelativeUri.create uri

    Assert.Equal(Uri(uri, UriKind.Relative), RelativeUri.systemUriOf actual)

[<Fact>]
let ``given an ivalid relative URI when create then throws an exception`` () =
    let uri = "http://localhost"

    let created =
        try
            RelativeUri.create uri |> ignore
            true
        with _ ->
            false

    if created then failwith "RelativeUri.create should have thrown an exception for invalid URI"

[<Fact>]
let ``given a valid relative URI when tryCreate then returns Some relative URI`` () =
    let uri = "api/customers"

    match RelativeUri.tryCreate uri with
    | Some actual -> Assert.Equal(Uri(uri, UriKind.Relative), RelativeUri.systemUriOf actual)
    | None -> failwith "RelativeUri.tryCreate should have succeeded for valid URI"

[<Fact>]
let ``given an invalid relative URI when tryCreate then returns None`` () =
    let uri = "https://localhost"

    match RelativeUri.tryCreate uri with
    | Some u -> failwith "RelativeUri.tryCreate should not have succeeded for invalid URI"
    | None -> ()

[<Fact>]
let ``given a valid absolute URI when create then systemUri of it is an absolute URI`` () =
    let uri = "http://localhost"

    let actual = AbsoluteUri.create uri

    Assert.Equal(Uri(uri, UriKind.Absolute), AbsoluteUri.systemUriOf actual)

[<Fact>]
let ``given an ivalid absolute URI when create then throws an exception`` () =
    let uri = "http://local host"

    let created =
        try
            AbsoluteUri.create uri |> ignore
            true
        with _ ->
            false

    if created then failwith "AbsoluteUri.create should have thrown an exception for invalid URI"

[<Fact>]
let ``given a valid absolute URI when tryCreate then returns Some absolute URI`` () =
    let uri = "http://localhost"

    match AbsoluteUri.tryCreate uri with
    | Some actual -> Assert.Equal(Uri(uri, UriKind.Absolute), AbsoluteUri.systemUriOf actual)
    | None -> failwith "AbsoluteUri.tryCreate should have succeeded for valid URI"

[<Fact>]
let ``given an invalid absolute URI when tryCreate then returns None`` () =
    let uri = "https//localhost"

    match AbsoluteUri.tryCreate uri with
    | Some u -> failwith "AbsoluteUri.tryCreate should not have succeeded for invalid URI"
    | None -> ()

[<Theory>]
[<InlineData("api", "customers", "api/customers")>]
[<InlineData("api/", "/customers", "api/customers")>]
[<InlineData("api/", "customers", "api/customers")>]
[<InlineData("api", "/customers", "api/customers")>]
let ``given two relative URIs when concat then returns concatenated URI`` left right expected =
    let left = RelativeUri.create left
    let right = RelativeUri.create right

    let actual = RelativeUri.concat left right

    Assert.Equal(Uri(expected, UriKind.Relative), RelativeUri.systemUriOf actual)

[<Theory>]
[<InlineData("http://localhost", "api", "http://localhost/api")>]
[<InlineData("http://localhost/", "/api", "http://localhost/api")>]
[<InlineData("http://localhost/", "api", "http://localhost/api")>]
[<InlineData("http://localhost", "/api", "http://localhost/api")>]
let ``given an absolute URI and a relative URI when concat then returns concatenated URI`` left right expected =
    let left = AbsoluteUri.create left
    let right = RelativeUri.create right

    let actual = AbsoluteUri.concat left right

    Assert.Equal(Uri(expected, UriKind.Absolute), AbsoluteUri.systemUriOf actual)

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
