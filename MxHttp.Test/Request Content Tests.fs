module MxHttp.RequestContentTests

open System
open System.Text.Json
open Xunit

module Json =

    let serialize object =
        JsonSerializer.Serialize(object)

type Customer = {
    Name : string
}

[<Fact>]
let ``given an object when serialized to JSON string then expected RequestContent is returned`` () =
    let customer = { Name = "any" }

    let actual = Request.Content.string Json.serialize MediaType.applicationJson customer

    Assert.Equal(StringContent (MediaType.applicationJson, """{"Name":"any"}"""), actual)

[<Fact>]
let ``given a byte array when byteArray then expected RequestContent is returned`` () =
    let bytes = [|1uy; 0uy; 255uy|]

    let actual = Request.Content.byteArray bytes

    Assert.Equal(ByteArrayContent bytes, actual)

[<Fact>]
let ``given a key value pairs when formUrlEncoded then expected RequestContent is returned`` () =
    let keyValuePairs = [
        "K1", "V1"
        "K2", "V2"
    ]
    let expected = [
        Key.ofString "K1", Value.ofString "V1"
        Key.ofString "K2", Value.ofString "V2"
    ]

    let actual = Request.Content.formUrlEncoded keyValuePairs

    Assert.Equal(FormUrlEncodedContent expected, actual)
