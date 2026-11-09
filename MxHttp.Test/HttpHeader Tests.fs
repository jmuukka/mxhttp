module MxHttp.HttpHeaderTests

open Xunit

[<Fact>]
let ``given no headers when applyHeaders with zero headers then has no headers`` () =
    use httpContent = new System.Net.Http.ByteArrayContent([|255uy|])
    let sut = httpContent.Headers

    HttpHeader.applyHeaders [] sut

    Assert.Equal([], sut)

[<Fact>]
let ``given new headers when applyHeaders then header is added`` () =
    use httpContent = new System.Net.Http.ByteArrayContent([|255uy|])
    let sut = httpContent.Headers
    let key = Key.ofString "k"
    let values = [Value.ofString "v1"; Value.ofString "v2"]

    HttpHeader.applyHeaders [key, values] sut

    Assert.Equal<Header list>([key, values], Header.ofHttpHeaders sut)

[<Fact>]
let ``given headers with existing key when applyHeaders then the header values are replaced with new values`` () =
    use httpContent = new System.Net.Http.ByteArrayContent([|255uy|])
    let sut = httpContent.Headers
    sut.Add("k1", ["v0"; "v1"])
    sut.Add("k2", ["v10"; "v11"])
    let key = Key.ofString "k1"
    let values = [Value.ofString "v1"; Value.ofString "v2"]
    let expected = [
        key, values
        Key.ofString "k2", [Value.ofString "v10"; Value.ofString "v11"]
    ]

    HttpHeader.applyHeaders [key, values] sut

    Assert.Equal<Header list>(expected, Header.ofHttpHeaders sut)
