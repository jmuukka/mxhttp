module MxHttp.ResponseTaskTests

open System
open System.Net
open System.Net.Http
open System.Text
open Xunit
open MxHttp

[<Fact>]
let ``given an OK HttpResponseMessage with empty content when ofHttpResponseMessage then returns Ok response with expected data`` () =
    let request = Request.get [] (Uri.relative "any")
    use responseMessage = new HttpResponseMessage(HttpStatusCode.OK)

    Response.Task.ofHttpResponseMessage request responseMessage
    |> Task.result
    |> Assert.okWith (fun response ->
        Assert.Equal(request, response.Request)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode)
        Assert.Equal<Header list>([], response.Headers)
        Assert.Equal<ResponseContent option>(None, response.Content)
    )

[<Fact>]
let ``given an OK HttpResponseMessage with empty plain text when ofHttpResponseMessage then returns Ok response with expected data`` () =
    let request = Request.get [] (Uri.relative "any")
    use responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
    responseMessage.Content <- new StringContent("", Encoding.UTF8, "text/plain")
    let expectedContentHeaders = [
        (Key.ofString "Content-Type", [Value.ofString "text/plain; charset=utf-8"])
    ]
    let expectedResponseContent = {
        Headers = expectedContentHeaders
        Content = StringContent (MediaType.ofString "text/plain", "")
    }

    Response.Task.ofHttpResponseMessage request responseMessage
    |> Task.result
    |> Assert.okWith (fun response ->
        Assert.Equal(request, response.Request)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode)
        Assert.Equal<Header list>([], response.Headers)
        Assert.Equal<ResponseContent option>(Some expectedResponseContent, response.Content)
    )

[<Fact>]
let ``given an OK HttpResponseMessage with empty octet-stream without Content-Length when ofHttpResponseMessage then returns Ok response with expected data`` () =
    let request = Request.get [] (Uri.relative "any")
    use responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
    responseMessage.Content <- new ByteArrayContent([||])
    responseMessage.Content.Headers.ContentType <- Headers.MediaTypeHeaderValue("application/octet-stream")
    let expectedContentHeaders = [
        (Key.ofString "Content-Type", [Value.ofString "application/octet-stream"])
    ]
    let expectedResponseContent = {
        Headers = expectedContentHeaders
        Content = ByteArrayContent [||]
    }

    Response.Task.ofHttpResponseMessage request responseMessage
    |> Task.result
    |> Assert.okWith (fun response ->
        Assert.Equal(request, response.Request)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode)
        Assert.Equal<Header list>([], response.Headers)
        Assert.Equal<ResponseContent option>(Some expectedResponseContent, response.Content)
    )

[<Fact>]
let ``given an OK HttpResponseMessage with one byte without Content-Type when ofHttpResponseMessage then returns Ok response with expected data`` () =
    let request = Request.get [] (Uri.relative "any")
    use responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
    responseMessage.Content <- new ByteArrayContent([|1uy|])
    let expectedResponseContent = {
        Headers = []
        Content = ByteArrayContent [|1uy|]
    }

    Response.Task.ofHttpResponseMessage request responseMessage
    |> Task.result
    |> Assert.okWith (fun response ->
        Assert.Equal(request, response.Request)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode)
        Assert.Equal<Header list>([], response.Headers)
        Assert.Equal<ResponseContent option>(Some expectedResponseContent, response.Content)
    )

[<Fact>]
let ``given an OK HttpResponseMessage with JSON string when ofHttpResponseMessage then returns Ok response with expected data`` () =
    let request = Request.get [] (Uri.relative "any")
    use responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
    let json = """{"Name":"test"}"""
    responseMessage.Content <- new StringContent(json, Encoding.UTF8, MediaType.toString MediaType.applicationJson)
    responseMessage.Content.Headers.ContentLength <- json.Length
    responseMessage.Headers.Add("Custom", ["value1"; "value2"])
    responseMessage.Headers.Add("Second", ["val3"])
    responseMessage.Headers.Add("Empty", []) // A key without values will be ignored.
    let expectedResponseHeaders = [
        Key.ofString "Custom", [Value.ofString "value1"; Value.ofString "value2"]
        Key.ofString "Second", [Value.ofString "val3"]
    ]
    let expectedContentHeaders = [
        Key.ofString "Content-Type", [Value.ofString "application/json; charset=utf-8"]
        Key.ofString "Content-Length", [Value.ofString (string json.Length)]
    ]
    let expectedResponseContent = {
        Headers = expectedContentHeaders
        Content = StringContent (MediaType.applicationJson, json)
    }

    Response.Task.ofHttpResponseMessage request responseMessage
    |> Task.result
    |> Assert.okWith (fun response ->
        Assert.Equal(request, response.Request)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode)
        Assert.Equal<Header list>(expectedResponseHeaders, response.Headers)
        Assert.Equal<ResponseContent option>(Some expectedResponseContent, response.Content)
    )

[<Fact>]
let ``given an OK HttpResponseMessage with byte array and content type when ofHttpResponseMessage then returns Ok response with expected data`` () =
    let request = Request.get [] (Uri.relative "any")
    use responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
    let bytes = [|1uy; 0uy; 255uy|]
    responseMessage.Content <- new ByteArrayContent(bytes)
    responseMessage.Content.Headers.ContentType <- Headers.MediaTypeHeaderValue("application/octet-stream")
    responseMessage.Content.Headers.ContentLength <- bytes.Length
    let expectedContentHeaders = [
        Key.ofString "Content-Type", [Value.ofString "application/octet-stream"]
        Key.ofString "Content-Length", [Value.ofString (string bytes.Length)]
    ]
    let expectedResponseContent = {
        Headers = expectedContentHeaders
        Content = ByteArrayContent bytes
    }

    Response.Task.ofHttpResponseMessage request responseMessage
    |> Task.result
    |> Assert.okWith (fun response ->
        Assert.Equal(request, response.Request)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode)
        Assert.Equal<Header list>([], response.Headers)
        Assert.Equal<ResponseContent option>(Some expectedResponseContent, response.Content)
    )

[<Fact>]
let ``given a Bad Request HttpResponseMessage when ofHttpResponseMessage then returns Error response with expected data`` () =
    let request = Request.get [] (Uri.relative "any")
    use response = new HttpResponseMessage(HttpStatusCode.BadRequest)
    let json = """{"Error":"test"}"""
    response.Content <- new StringContent(json, Encoding.UTF8, MediaType.toString MediaType.applicationJson)
    let expectedContentHeaders = [
        (Key.ofString "Content-Type", [Value.ofString "application/json; charset=utf-8"])
    ]

    Response.Task.ofHttpResponseMessage request response
    |> Task.result
    |> Assert.errorWithNonSuccessHttpStatusCode (fun response ->
        Assert.Equal(request, response.Request)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        Assert.Equal<Header list>([], response.Headers)
        Assert.Equal<ResponseContent option>(Some {
            Headers = expectedContentHeaders
            Content = StringContent (MediaType.applicationJson, json) }, response.Content)
    )

[<Fact>]
let ``given ofHttpResponseMessage throws when to ofHttpResponseMessage then returns Error response with expected data`` () =
    let request = Request.get [] (Uri.relative "any")

    Unchecked.defaultof<HttpResponseMessage>
    |> Response.Task.ofHttpResponseMessage request
    |> Task.result
    |> Assert.errorWith (fun error ->
        match error with
        | ResponseContentException (req, httpResponseMessage, ex) ->
            Assert.Equal(request, req)
            Assert.Null(httpResponseMessage)
            Assert.IsType<NullReferenceException>(ex)
        | _ -> failwithf "%A" error
    )
