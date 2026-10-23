module MxHttp.RequestTests

open System
open System.Linq
open System.Net.Http
open Xunit

let stringContent = RequestContent.string Json.serialize MediaType.applicationJson { Name = "any" }
let bytes = [|1uy; 0uy; 255uy|]
let byteArrayContent = RequestContent.byteArray bytes
let formUrlEncodedContent = RequestContent.formUrlEncoded [
    "K1", "V1"
    "K2", "V2"
]

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

    let actual = Request.post headers uri stringContent

    Assert.Equal({Method = HttpMethod.Post; ResourceUri = uri; Headers = headers; Content = Some stringContent}, actual)

[<Fact>]
let ``given headers, URI and content when put then returns expected Request`` () =
    let headers = [Header.acceptJson]
    let uri = Uri.absolute "https://localhost/api/customers/1"

    let actual = Request.put headers uri stringContent

    Assert.Equal({Method = HttpMethod.Put; ResourceUri = uri; Headers = headers; Content = Some stringContent}, actual)

[<Fact>]
let ``given headers, URI and content when patch then returns expected Request`` () =
    let headers = [Header.acceptJson]
    let uri = Uri.absolute "https://localhost/api/customers/1"

    let actual = Request.patch headers uri stringContent

    Assert.Equal({Method = HttpMethod.patch; ResourceUri = uri; Headers = headers; Content = Some stringContent}, actual)

[<Fact>]
let ``given headers and URI when delete then returns expected Request`` () =
    let headers = [Header.acceptJson]
    let uri = Uri.absolute "https://localhost/api/customers/1"

    let actual = Request.delete headers uri

    Assert.Equal({Method = HttpMethod.Delete; ResourceUri = uri; Headers = headers; Content = None}, actual)

[<Fact>]
let ``given a Request without content when toHttpRequestMessage then returns expected HttpRequestMessage`` () =
    let headers = [Header.acceptXml]
    let uri = Uri.absolute "https://localhost/api/customers/1"
    let request = {Method = HttpMethod.Get; ResourceUri = uri; Headers = headers; Content = None}

    use actual = Request.toHttpRequestMessage request

    Assert.Null(actual.Content)
    Assert.Equal(MediaType.toString MediaType.applicationXml, (Seq.head actual.Headers.Accept).MediaType)
    Assert.Equal(HttpMethod.Get, actual.Method)
    Assert.Equal(0, actual.Options.Count())
    Assert.Equal(Uri.toSystemUri uri, actual.RequestUri)
    Assert.Equal(new Version(1, 1), actual.Version)
    Assert.Equal(HttpVersionPolicy.RequestVersionOrLower, actual.VersionPolicy)

[<Fact>]
let ``given a Request with string content when toHttpRequestMessage then returns expected HttpRequestMessage`` () =
    let headers = [Header.acceptJson]
    let uri = Uri.absolute "https://localhost/api/customers/1"
    let request = {Method = HttpMethod.Post; ResourceUri = uri; Headers = headers; Content = Some stringContent}

    use actual = Request.toHttpRequestMessage request

    Assert.Equal("utf-8", actual.Content.Headers.ContentType.CharSet)
    Assert.Equal(MediaType.toString MediaType.applicationJson, actual.Content.Headers.ContentType.MediaType)
    Assert.Equal("""{"Name":"any"}""", actual.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously)
    Assert.Equal(Nullable<int64>(14L), actual.Content.Headers.ContentLength)
    Assert.Equal(MediaType.toString MediaType.applicationJson, (Seq.head actual.Headers.Accept).MediaType)
    Assert.Equal(HttpMethod.Post, actual.Method)
    Assert.Equal(0, actual.Options.Count())
    Assert.Equal(Uri.toSystemUri uri, actual.RequestUri)
    Assert.Equal(new Version(1, 1), actual.Version)
    Assert.Equal(HttpVersionPolicy.RequestVersionOrLower, actual.VersionPolicy)

[<Fact>]
let ``given a Request with byte array when toHttpRequestMessage then returns expected HttpRequestMessage`` () =
    let uri = Uri.absolute "https://localhost/api/customers/1/images/2"
    let request = {Method = HttpMethod.Put; ResourceUri = uri; Headers = []; Content = Some byteArrayContent}

    use actual = Request.toHttpRequestMessage request

    Assert.Equal(MediaType.toString MediaType.applicationOctetStream, actual.Content.Headers.ContentType.MediaType)
    Assert.Equal<byte array>(bytes, actual.Content.ReadAsByteArrayAsync() |> Async.AwaitTask |> Async.RunSynchronously)
    Assert.Equal(Nullable<int64>(bytes.Length), actual.Content.Headers.ContentLength)
    Assert.Equal(HttpMethod.Put, actual.Method)
    Assert.Equal(0, actual.Options.Count())
    Assert.Equal(Uri.toSystemUri uri, actual.RequestUri)
    Assert.Equal(new Version(1, 1), actual.Version)
    Assert.Equal(HttpVersionPolicy.RequestVersionOrLower, actual.VersionPolicy)

[<Fact>]
let ``given a Request with form url encoded content when toHttpRequestMessage then returns expected HttpRequestMessage`` () =
    let headers = [Header.accept MediaType.applicationFormUrlEncoded]
    let uri = Uri.absolute "https://localhost/api/customers/1/custom-data"
    let request = {Method = HttpMethod.Post; ResourceUri = uri; Headers = headers; Content = Some formUrlEncodedContent}

    use actual = Request.toHttpRequestMessage request

    Assert.Equal(MediaType.toString MediaType.applicationFormUrlEncoded, actual.Content.Headers.ContentType.MediaType)
    Assert.Equal("K1=V1&K2=V2", actual.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously)
    Assert.Equal(Nullable<int64>(11L), actual.Content.Headers.ContentLength)
    Assert.Equal(HttpMethod.Post, actual.Method)
    Assert.Equal(0, actual.Options.Count())
    Assert.Equal(Uri.toSystemUri uri, actual.RequestUri)
    Assert.Equal(new Version(1, 1), actual.Version)
    Assert.Equal(HttpVersionPolicy.RequestVersionOrLower, actual.VersionPolicy)
