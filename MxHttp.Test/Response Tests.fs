module MxHttp.ResponseTests

open System
open System.Net
open System.Net.Http
open System.Text
open Xunit

[<Fact>]
let ``given an OK HttpResponseMessage when to Response then returns Ok response with expected data`` () =
    let request = Request.get [] (Uri.relative "any")
    use response = new HttpResponseMessage(HttpStatusCode.OK)
    let json = """{"Name":"test"}"""
    response.Content <- new StringContent(json, Encoding.UTF8, MediaType.toString MediaType.applicationJson)
    response.Headers.Add("Custom", ["value1"; "value2"]);
    let expectedResponseHeaders = [
        Key.ofString "Custom", [Value.ofString "value1"; Value.ofString "value2"]
    ]
    let expectedContentHeaders = [
        Key.ofString "Content-Type", [Value.ofString "application/json; charset=utf-8"]
        Key.ofString "Content-Length", [Value.ofString (string json.Length)]
    ]

    let actual = Response.ofHttpResponseMessage request response

    match actual with
    | Ok response ->
        Assert.Equal(request, response.Request)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode)
        Assert.Equal<Header list>(expectedResponseHeaders, response.Headers)
        Assert.Equal<ResponseContent option>(Some {
            Headers = expectedContentHeaders
            Data = StringResponseContent json }, response.Content)
    | Error error ->
        failwithf "%A" error

[<Fact>]
let ``given a Bad Request HttpResponseMessage when to Response then returns Error response with expected data`` () =
    let request = Request.get [] (Uri.relative "any")
    use response = new HttpResponseMessage(HttpStatusCode.BadRequest)
    let json = """{"Error":"test"}"""
    response.Content <- new StringContent(json, Encoding.UTF8, MediaType.toString MediaType.applicationJson)
    let expectedContentHeaders = [
        Key.ofString "Content-Type", [Value.ofString "application/json; charset=utf-8"]
        Key.ofString "Content-Length", [Value.ofString (string json.Length)]
    ]

    let actual = Response.ofHttpResponseMessage request response

    match actual with
    | Ok _ ->
        failwith "Bad Request should map to an error case."
    | Error (NonSuccessHttpStatusCode response) ->
        Assert.Equal(request, response.Request)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        Assert.Equal<Header list>([], response.Headers)
        Assert.Equal<ResponseContent option>(Some {
            Headers = expectedContentHeaders
            Data = StringResponseContent json }, response.Content)
    | Error error ->
        failwithf "%A" error

// TODO need to refactor to be able to test ResponseContentException

open System.Text.Json

module Json =

    let deserialize<'t> (json : string) =
        JsonSerializer.Deserialize<'t>(json)

type Customer = {
    Name : string
}

[<Fact>]
let ``given a JSON response when parse then expected object returned`` () =
    let deserializers = [
        MediaType.applicationJson, Json.deserialize
    ]
    let json = """{"Name":"Any"}"""
    let response = {
        StatusCode = HttpStatusCode.OK
        Headers = []
        Content = Some {
            Headers = [
                Key.ofString "Content-Type", [Value.ofString "application/json; charset=utf-8"]
                Key.ofString "Content-Length", [Value.ofString (string json.Length)]
            ]
            Data = StringResponseContent json
        }
        Request = Request.get [] (Uri.relative "any")
    }

    let actual = Response.parse<Customer> deserializers response

    match actual with  
    | Ok customer ->
        Assert.Equal("Any", customer.Name)
    | Error error ->
        failwithf "%A" error
