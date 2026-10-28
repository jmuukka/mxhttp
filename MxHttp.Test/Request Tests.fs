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
