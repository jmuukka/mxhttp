module MxHttp.HttpResponseMessageTests

open System.Net
open System.Net.Http
open Xunit

[<Fact>]
let ``given HttpResponseMessage with status code when statusCode then returns expected status code`` () =
    let actual = new HttpResponseMessage(HttpStatusCode.OK)

    Assert.Equal(HttpStatusCode.OK, HttpResponseMessage.statusCode actual)

[<Fact>]
let ``given HttpResponseMessage with headers when headers then returns expected headers`` () =
    let actual = new HttpResponseMessage(HttpStatusCode.OK)
    actual.Headers.Add("X-Test-Header", "TestValue1")
    actual.Headers.Add("X-Test-Header", "TestValue2")

    let expectedKey = Key.ofString "X-Test-Header"
    let expectedValues = [Value.ofString "TestValue1"; Value.ofString "TestValue2"]
    Assert.Equal<Header list>([expectedKey, expectedValues], HttpResponseMessage.headers actual)

[<Fact>]
let ``given HttpResponseMessage with content when httpContent then returns expected content`` () =
    let httpContent = new StringContent("Test Content")
    let actual = new HttpResponseMessage(HttpStatusCode.OK)
    actual.Content <- httpContent

    Assert.Same(httpContent, HttpResponseMessage.httpContent actual)
