module MxHttp.HttpContentTests

open System
open Xunit
open System.IO
open System.Net.Http

[<Fact>]
let ``given a string when constructed then expected HttpContent is returned`` () =
    let json = """{"Name":"any"}"""

    use actual = HttpContent.ofString MediaType.applicationJson json

    Assert.Equal(MediaType.applicationJson, MediaType.ofString actual.Headers.ContentType.MediaType)
    Assert.Equal(json, actual.ReadAsStringAsync() |> Task.result)
    Assert.Equal(Nullable<int64> json.Length, actual.Headers.ContentLength)

[<Fact>]
let ``given a byte array when constructed then expected HttpContent is returned`` () =
    let bytes = [|1uy; 0uy; 255uy|]

    use actual = HttpContent.ofByteArray bytes

    Assert.Equal<byte array>(bytes, actual.ReadAsByteArrayAsync() |> Task.result)
    Assert.Equal(Nullable<int64> bytes.Length, actual.Headers.ContentLength)

[<Fact>]
let ``given a key value pairs when constructed then expected HttpContent is returned`` () =
    let keyValuePairs = [
        Key.ofString "K1", Value.ofString "V1"
        Key.ofString "K2", Value.ofString "V2"
    ]
    let expected = "K1=V1&K2=V2"

    use actual = HttpContent.ofKeyValuePairs keyValuePairs

    Assert.Equal(MediaType.applicationFormUrlEncoded, MediaType.ofString actual.Headers.ContentType.MediaType)
    Assert.Equal(expected, actual.ReadAsStringAsync() |> Task.result)
    Assert.Equal(Nullable<int64> expected.Length, actual.Headers.ContentLength)

[<Fact>]
let ``given a byte array stream when constructed then expected HttpContent is returned`` () =
    let bytes = [|1uy; 0uy; 255uy|]
    use stream = new MemoryStream(bytes)

    use actual = HttpContent.ofStream MediaType.applicationOctetStream stream

    Assert.Equal(MediaType.applicationOctetStream, MediaType.ofString actual.Headers.ContentType.MediaType)
    Assert.Equal<byte array>(bytes, actual.ReadAsByteArrayAsync() |> Task.result)
    Assert.Equal(Nullable<int64> bytes.Length, actual.Headers.ContentLength)

[<Fact>]
let ``given no headers when applyHeaders zero headers then HttpContent has no headers`` () =
    let bytes = [|1uy; 0uy; 255uy|]
    use httpContent = new ByteArrayContent(bytes)

    let actual = HttpContent.applyHeaders [] httpContent

    Assert.Same(httpContent, actual)
    Assert.Equal([], actual.Headers)

[<Fact>]
let ``given new headers when applyHeaders then header is added`` () =
    let bytes = [|1uy; 0uy; 255uy|]
    use httpContent = new ByteArrayContent(bytes)
    let key = Key.ofString "k"
    let values = [Value.ofString "v1"; Value.ofString "v2"]

    let actual = HttpContent.applyHeaders [key, values] httpContent

    Assert.Same(httpContent, actual)
    Assert.Equal<Header list>([key, values], Header.ofHttpHeaders actual.Headers)

[<Fact>]
let ``given headers with existing key when applyHeaders then the header values are replaced with new values`` () =
    let bytes = [|1uy; 0uy; 255uy|]
    use httpContent = new ByteArrayContent(bytes)
    httpContent.Headers.Add("k", ["v0"; "v1"])
    let key = Key.ofString "k"
    let values = [Value.ofString "v1"; Value.ofString "v2"]

    let actual = HttpContent.applyHeaders [key, values] httpContent

    Assert.Same(httpContent, actual)
    Assert.Equal<Header list>([key, values], Header.ofHttpHeaders actual.Headers)
