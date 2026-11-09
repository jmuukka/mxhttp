module MxHttp.HttpResponseContentAsyncTests

open System.IO
open System.Net
open System.Net.Http
open System.Threading
open Xunit

[<Fact>]
let ``given an HttpContent with a byte array when readAsStream then the returned stream contains the same bytes`` () =
    let bytes = [|1uy; 0uy; 255uy|]
    use requestStream = new MemoryStream(bytes)
    let httpContent = HttpContent.ofStream MediaType.applicationOctetStream requestStream

    let actual =
        HttpResponseContent.Async.readAsStream httpContent
        |> Async.run

    let actualBytes = Http.readStreamAsByteArray 100 actual
    Assert.Equal<byte array>(bytes, actualBytes)

[<Fact>]
let ``given an HttpContent with a byte array when readAsByteArray then returns the same bytes`` () =
    let bytes = [|1uy; 0uy; 255uy|]
    let httpContent = HttpContent.ofByteArray bytes

    let actual =
        HttpResponseContent.Async.readAsByteArray httpContent
        |> Async.run

    Assert.Equal<byte array>(bytes, actual)

[<Fact>]
let ``given an HttpContent with a string when readAsString then returns the same string`` () =
    let string = "any string"
    let httpContent = HttpContent.ofString (MediaType.ofString "text/plain") string

    let actual =
        HttpResponseContent.Async.readAsString httpContent
        |> Async.run

    Assert.Equal(string, actual)

[<Fact>]
let ``given an HttpContent with a JSON string when parse then returns an expected object`` () =
    let deserializers =
        Map.empty.Add(MediaType.applicationJson, Json.deserialize)
    let string = """{"Name": "any string"}"""
    let httpContent = HttpContent.ofString MediaType.applicationJson string

    let actual =
        HttpResponseContent.Async.parse<CustomerRequest> deserializers httpContent
        |> Async.run
        |> Result.ok

    Assert.Equal({ Name = "any string" }, actual)

[<Fact>]
let ``given an HttpContent with a JSON string and HttpContent does not have media type when parse then returns an expected error`` () =
    let deserializers =
        Map.empty.Add(MediaType.applicationXml, Json.deserialize)
    let httpContent = new ByteArrayContent([|111uy|])

    HttpResponseContent.Async.parse<CustomerRequest> deserializers httpContent
    |> Async.run
    |> Assert.errorWith (
        function
        | ParseError (httpContent, errorMessage) ->
            Assert.Equal(httpContent, httpContent)
            Assert.Equal("The HTTP response content does not have a media type.", errorMessage)
        | err ->
            failwithf "Did not expect %A" err
    )

[<Fact>]
let ``given an HttpContent with a JSON string and deserializers not having JSON media type when parse then returns an expected error`` () =
    let deserializers =
        Map.empty.Add(MediaType.applicationXml, Json.deserialize)
    let string = """{"Name": "any string"}"""
    let httpContent = HttpContent.ofString MediaType.applicationJson string

    HttpResponseContent.Async.parse<CustomerRequest> deserializers httpContent
    |> Async.run
    |> Assert.errorWith (
        function
        | ParseError (httpContent, errorMessage) ->
            Assert.Equal(httpContent, httpContent)
            Assert.Equal($"Deserializer for media type {MediaType.toString MediaType.applicationJson} is not configured.", errorMessage)
        | err ->
            failwithf "Did not expect %A" err
    )

[<Fact>]
let ``given an HttpContent with an invalid JSON string when parse then returns an expected error`` () =
    let deserializers =
        Map.empty.Add(MediaType.applicationJson, Json.deserialize)
    let string = """<html>error</html>"""
    let httpContent = HttpContent.ofString MediaType.applicationJson string

    HttpResponseContent.Async.parse<CustomerRequest> deserializers httpContent
    |> Async.run
    |> Assert.errorWith (
        function
        | DeserializeException (httpContent, ex) ->
            Assert.Equal(httpContent, httpContent)
            Assert.True(ex.Message.StartsWith("""'<' is an invalid start of a value."""))
        | err ->
            failwithf "Did not expect %A" err
    )
