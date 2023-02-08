module MxHttp.RequestTests

open System
open System.Linq
open System.Net.Http
open System.Text.Json
open Xunit

module Json =

    let serialize object =
        JsonSerializer.Serialize(object)

type Customer = {
    Name : string
}

let content = Request.Content.string Json.serialize MediaType.applicationJson { Name = "any" }

[<Fact>]
let ``given headers and URI when get then returns expected Request`` () =
    let headers = [Header.acceptJson]
    let uri = Uri.absolute "https://localhost/api/customers/1"

    let actual = Request.get headers uri

    Assert.Equal({Method = HttpMethod.Get; ResourceUri = uri; Headers = headers; Content = None}, actual)

[<Fact>]
let ``given headers, URI and content when post then returns expected Request`` () =
    let headers = [Header.acceptJson]
    let uri = Uri.absolute "https://localhost/api/customers"

    let actual = Request.post headers uri content

    Assert.Equal({Method = HttpMethod.Post; ResourceUri = uri; Headers = headers; Content = Some content}, actual)

[<Fact>]
let ``given headers, URI and content when put then returns expected Request`` () =
    let headers = [Header.acceptJson]
    let uri = Uri.absolute "https://localhost/api/customers/1"

    let actual = Request.put headers uri content

    Assert.Equal({Method = HttpMethod.Put; ResourceUri = uri; Headers = headers; Content = Some content}, actual)

[<Fact>]
let ``given headers, URI and content when patch then returns expected Request`` () =
    let headers = [Header.acceptJson]
    let uri = Uri.absolute "https://localhost/api/customers/1"

    let actual = Request.patch headers uri content

    Assert.Equal({Method = HttpMethod.patch; ResourceUri = uri; Headers = headers; Content = Some content}, actual)

[<Fact>]
let ``given headers and URI when delete then returns expected Request`` () =
    let headers = [Header.acceptJson]
    let uri = Uri.absolute "https://localhost/api/customers/1"

    let actual = Request.delete headers uri

    Assert.Equal({Method = HttpMethod.Delete; ResourceUri = uri; Headers = headers; Content = None}, actual)

[<Fact>]
let ``given a Request when toHttpRequestMessage then returns expected HttpRequestMessage`` () =
    let headers = [Header.acceptJson]
    let uri = Uri.absolute "https://localhost/api/customers/1"
    let request = {Method = HttpMethod.Post; ResourceUri = uri; Headers = headers; Content = Some content}

    use actual = Request.toHttpRequestMessage request

    Assert.Equal("utf-8", actual.Content.Headers.ContentType.CharSet)
    Assert.Equal(MediaType.toString MediaType.applicationJson, actual.Content.Headers.ContentType.MediaType)
    Assert.Equal("""{"Name":"any"}""", actual.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously)
    Assert.Equal(MediaType.toString MediaType.applicationJson, (Seq.head actual.Headers.Accept).MediaType)
    Assert.Equal(HttpMethod.Post, actual.Method)
    Assert.Equal(0, actual.Options.Count())
    Assert.Equal(Uri.systemUriOf uri, actual.RequestUri)
    Assert.Equal(new Version(1, 1), actual.Version)
    Assert.Equal(HttpVersionPolicy.RequestVersionOrLower, actual.VersionPolicy)
