module MxHttp.ContentTests

open Xunit
open System

[<Fact>]
let ``given an object when serialized to JSON string then expected Content is returned`` () =
    let customer = { Name = "any" }

    let actual = Content.string Json.serialize MediaType.applicationJson customer

    Assert.Equal(StringContent (MediaType.applicationJson, """{"Name":"any"}"""), actual)

[<Fact>]
let ``given a byte array when byteArray then expected Content is returned`` () =
    let bytes = [|1uy; 0uy; 255uy|]

    let actual = Content.byteArray bytes

    Assert.Equal(ByteArrayContent bytes, actual)

[<Fact>]
let ``given a key value pairs when formUrlEncoded then expected Content is returned`` () =
    let keyValuePairs = [
        Key.ofString "K1", Value.ofString "V1"
        Key.ofString "K2", Value.ofString "V2"
    ]

    let actual = Content.formUrlEncoded keyValuePairs

    Assert.Equal(FormUrlEncodedContent keyValuePairs, actual)

[<Fact>]
let ``given StringContent when toHttpContent then returns expected HttpContent`` () =
    let json = """{"Name":"any"}"""
    let content = StringContent (MediaType.applicationJson, json)

    let actual = Content.toHttpContent content

    Assert.Equal(MediaType.toString MediaType.applicationJson, actual.Headers.ContentType.MediaType)
    Assert.Equal(Nullable<int64> json.Length, actual.Headers.ContentLength)
    Assert.Equal(json, actual.ReadAsStringAsync() |> Task.result)

[<Fact>]
let ``given ByteArrayContent when toHttpContent then returns expected HttpContent`` () =
    let bytes = [|1uy; 0uy; 255uy|]
    let content = ByteArrayContent bytes

    let actual = Content.toHttpContent content

    Assert.Equal(MediaType.toString MediaType.applicationOctetStream, actual.Headers.ContentType.MediaType)
    Assert.Equal(Nullable<int64> bytes.Length, actual.Headers.ContentLength)
    Assert.Equal<byte array>(bytes, actual.ReadAsByteArrayAsync() |> Task.result)

[<Fact>]
let ``given FormUrlEncodedContent when toHttpContent then returns expected HttpContent`` () =
    let keyValuePairs = [
        Key.ofString "K1", Value.ofString "V1"
        Key.ofString "K2", Value.ofString "V2"
    ]
    let expected = "K1=V1&K2=V2"
    let content = FormUrlEncodedContent keyValuePairs

    let actual = Content.toHttpContent content

    Assert.Equal(MediaType.toString MediaType.applicationFormUrlEncoded, actual.Headers.ContentType.MediaType)
    Assert.Equal(Nullable<int64> expected.Length, actual.Headers.ContentLength)
    Assert.Equal(expected, actual.ReadAsStringAsync() |> Task.result)
