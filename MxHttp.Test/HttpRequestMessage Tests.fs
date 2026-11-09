module MxHttp.HttpRequestMessageTests

open System
open System.Linq
open System.Net.Http
open Xunit

let readContent (httpRequestMessage: HttpRequestMessage) =
    let stream =
        httpRequestMessage.Content.ReadAsStreamAsync()
        |> Async.AwaitTask
        |> Async.run
    Http.readStreamAsByteArray 100 stream

[<Fact>]
let ``given HttpMethod when patch then returns expected HttpMethod`` () =
    let actual = HttpMethod.patch

    Assert.Equal("PATCH", actual.Method)

[<Fact>]
let ``given HttpMethod when query then returns expected HttpMethod`` () =
    let actual = HttpMethod.query

    Assert.Equal("QUERY", actual.Method)

[<Fact>]
let ``given an HttpMethod and an URI when create then returns expected HttpRequestMessage`` () =
    let uri = Uri.absolute "https://localhost/api/customers/1/images/2"

    use actual = HttpRequestMessage.create HttpMethod.Delete uri

    Assert.Null(actual.Content)
    Assert.Equal(HttpMethod.Delete, actual.Method)
    Assert.Equal(0, actual.Options.Count())
    Assert.Equal(Uri.toSystemUri uri, actual.RequestUri)
    Assert.Equal(new Version(1, 1), actual.Version)
    Assert.Equal(HttpVersionPolicy.RequestVersionOrLower, actual.VersionPolicy)

[<Fact>]
let ``given headers when applyHeaders then returns HttpRequestMessage with expected headers`` () =
    let uri = Uri.absolute "https://localhost/api/customers/1/images/2"
    use httpRequestMessage = HttpRequestMessage.create HttpMethod.query uri
    let headers = [
        Header.singleValue "key1" "value1"
    ]

    let actual = HttpRequestMessage.applyHeaders headers httpRequestMessage

    Assert.Same(httpRequestMessage, actual)
    Assert.Equal<Header list>(headers, Header.ofHttpHeaders actual.Headers)

[<Fact>]
let ``given two headers with the same key when applyHeaders then returns HttpRequestMessage with latter values`` () =
    let uri = Uri.absolute "https://localhost/api/customers/1/images/2"
    use httpRequestMessage = HttpRequestMessage.create HttpMethod.query uri
    httpRequestMessage.Headers.Add("key1", ["v0"])
    httpRequestMessage.Headers.Add("key2", ["v120"])
    let sourceHeaders = [
        Header.singleValue "key1" "value1"
        Header.singleValue "key1" "value2"
    ]
    let expectedHeaders = [
        Header.multipleValues "key1" ["value2"]
        Header.multipleValues "key2" ["v120"]
    ]

    let actual = HttpRequestMessage.applyHeaders sourceHeaders httpRequestMessage

    Assert.Same(httpRequestMessage, actual)
    Assert.Equal<Header list>(expectedHeaders, Header.ofHttpHeaders actual.Headers)

[<Fact>]
let ``given an HttpContent with a stream when ofHttpContent then returns expected HttpRequestMessage`` () =
    let bytes = [|1uy; 0uy; 255uy|]
    use stream = new System.IO.MemoryStream(bytes)
    let httpContent = HttpContent.ofStream MediaType.applicationOctetStream stream
    let headers = [Header.bearerAuthorization "API-token"]
    let uri = Uri.absolute "https://localhost/api/customers/1/images/2"

    use actual = HttpRequestMessage.ofHttpContent HttpMethod.Put uri headers httpContent

    Assert.Equal(MediaType.toString MediaType.applicationOctetStream, actual.Content.Headers.ContentType.MediaType)
    Assert.Equal<byte array>(bytes, readContent actual)
    Assert.Equal(Nullable<int64>(bytes.Length), actual.Content.Headers.ContentLength)
    Assert.Equal(HttpMethod.Put, actual.Method)
    Assert.Equal(0, actual.Options.Count())
    Assert.Equal(Uri.toSystemUri uri, actual.RequestUri)
    Assert.Equal(new Version(1, 1), actual.Version)
    Assert.Equal(HttpVersionPolicy.RequestVersionOrLower, actual.VersionPolicy)
