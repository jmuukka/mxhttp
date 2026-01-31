module MxHttp.ResponseTests

open System.Net
open System.Net.Http
open System.Text
open Xunit

[<Fact>]
let ``given an OK HttpResponseMessage when toResponse then returns Ok response with expected data`` () =
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

    async {
        return! Response.ofHttpResponseMessage request response
    }
    |> Async.RunSynchronously
    |> Assert.okWith (fun response ->
        Assert.Equal(request, response.Request)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode)
        Assert.Equal<Header list>(expectedResponseHeaders, response.Headers)
        Assert.Equal<ResponseContent option>(Some {
            Headers = expectedContentHeaders
            Data = StringResponseContent json }, response.Content)
    )

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

    let actual =
        async {
            return! Response.ofHttpResponseMessage request response
        }
        |> Async.RunSynchronously

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

    Response.parse<CustomerResponse> deserializers response
    |> Assert.okWith (fun customer ->
        Assert.Equal("Any", customer.Name)
    )
