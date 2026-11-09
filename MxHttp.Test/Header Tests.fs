module MxHttp.HeaderTests

open System
open System.Text
open Xunit

[<Fact>]
let ``when Key ofString then toString returns expected string`` () =
    let actual = Key.ofString "k"

    Assert.Equal("k", Key.toString actual)

[<Fact>]
let ``when Value ofString then toString returns expected string`` () =
    let actual = Value.ofString "k"

    Assert.Equal("k", Value.toString actual)

[<Fact>]
let ``when acceptJson then returns correct header`` () =
    let actual = Header.acceptJson

    Assert.Equal((Key.ofString "Accept", [Value.ofString "application/json"]), actual)

[<Fact>]
let ``when acceptXml then returns correct header`` () =
    let actual = Header.acceptXml

    Assert.Equal((Key.ofString "Accept", [Value.ofString "application/xml"]), actual)

[<Fact>]
let ``when accept media type then returns correct header`` () =
    let actual = Header.accept (MediaType.ofString "text/html")

    Assert.Equal((Key.ofString "Accept", [Value.ofString "text/html"]), actual)

[<Fact>]
let ``when bearerAuthorization then returns correct header`` () =
    let actual = Header.bearerAuthorization "my token"

    Assert.Equal((Key.ofString "Authorization", [Value.ofString "Bearer my token"]), actual)

[<Fact>]
let ``when basicAuthorization then returns correct header`` () =
    let creds = "it's me", "and my password"
    let expected =
        sprintf "%s:%s" (fst creds) (snd creds)
        |> Encoding.UTF8.GetBytes
        |> Convert.ToBase64String

    let actual = Header.basicAuthorization creds

    Assert.Equal((Key.ofString "Authorization", [Value.ofString $"Basic {expected}"]), actual)

[<Fact>]
let ``when authorization then returns correct header`` () =
    let tokenType = "Custom"
    let accessToken = "token"

    let actual = Header.authorization tokenType accessToken

    Assert.Equal((Key.ofString "Authorization", [Value.ofString $"Custom token"]), actual)

[<Fact>]
let ``given key and value map when ofHttpHeaders then returns Key Value list`` () =
    let headers =
        Map.empty
            .Add("k1", seq { "v1"; "v2" })
            .Add("k2", seq { })
    let expected = [
        Key.ofString "k1", [Value.ofString "v1"; Value.ofString "v2"]
        Key.ofString "k2", []
    ]

    let actual = Header.ofHttpHeaders headers

    Assert.Equal<(Key * Value list) list>(expected, actual)
