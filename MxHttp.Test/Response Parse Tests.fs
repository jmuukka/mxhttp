module MxHttp.ResponseParseTests

open System
open System.Net
open Xunit

[<Fact>]
let ``given a JSON response when parse then expected object returned`` () =
    let deserializers =
        Map.empty.Add(MediaType.applicationJson, Json.deserialize)
    let json = """{"Name":"Any"}"""
    let expedtedCustomer = {
        Id = Guid.Empty
        Name = "Any"
    }
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
        Assert.Equal(expedtedCustomer, customer)
    )

[<Fact>]
let ``given a response without content when parse then expected error returned`` () =
    let response = {
        StatusCode = HttpStatusCode.OK
        Headers = []
        Content = None
        Request = Request.get [] (Uri.relative "any")
    }

    Response.parse<CustomerResponse> Map.empty response
    |> Assert.errorWith (fun error ->
        match error with
        | ParseError (actualResponse, errorText) ->
            Assert.Equal(response, actualResponse)
            Assert.Equal("There is no content.", errorText)
        | _ -> failwithf "%A" error
    )

[<Fact>]
let ``given a response with content but without a content type when parse then expected error returned`` () =
    let response = {
        StatusCode = HttpStatusCode.OK
        Headers = []
        Content = Some {
            Headers = []
            Data = StringResponseContent "any"
        }
        Request = Request.get [] (Uri.relative "any")
    }

    Response.parse<CustomerResponse> Map.empty response
    |> Assert.errorWith (fun error ->
        match error with
        | ParseError (actualResponse, errorText) ->
            Assert.Equal(response, actualResponse)
            Assert.Equal("Unknown media type.", errorText)
        | _ -> failwithf "%A" error
    )

[<Fact>]
let ``given a byte array when parse then expected error returned`` () =
    let deserializers =
        Map.empty.Add(MediaType.applicationOctetStream, Json.deserialize)
    let response = {
        StatusCode = HttpStatusCode.OK
        Headers = []
        Content = Some {
            Headers = [
                (Key.ofString "Content-Type", [Value.ofString "application/octet-stream"])
            ]
            Data = ByteArrayResponseContent [|255uy|]
        }
        Request = Request.get [] (Uri.relative "any")
    }

    Response.parse<CustomerResponse> deserializers response
    |> Assert.errorWith (fun error ->
        match error with
        | ParseError (actualResponse, errorText) ->
            Assert.Equal(response, actualResponse)
            Assert.Equal("The content is a byte array. Cannot parse byte array to an object.", errorText)
        | _ -> failwithf "%A" error
    )

[<Fact>]
let ``given a missing deserailizer when parse then expected error returned`` () =
    let deserializers =
        Map.empty.Add(MediaType.applicationJson, Json.deserialize)
    let response = {
        StatusCode = HttpStatusCode.OK
        Headers = []
        Content = Some {
            Headers = [
                (Key.ofString "Content-Type", [Value.ofString "text/plain"])
            ]
            Data = StringResponseContent "any text"
        }
        Request = Request.get [] (Uri.relative "any")
    }

    Response.parse<CustomerResponse> deserializers response
    |> Assert.errorWith (fun error ->
        match error with
        | ParseError (actualResponse, errorText) ->
            Assert.Equal(response, actualResponse)
            Assert.Equal("Deserializer for media type text/plain is not configured.", errorText)
        | _ -> failwithf "%A" error
    )

[<Fact>]
let ``given a deserializer throws an exception when parse then expected error returned`` () =
    let crashingDeserializer s = raise (InvalidOperationException())
    let deserializers =
        Map.empty.Add(MediaType.applicationJson, crashingDeserializer)
    let json = """{"Name":"Any"}"""
    let response = {
        StatusCode = HttpStatusCode.OK
        Headers = []
        Content = Some {
            Headers = [
                (Key.ofString "Content-Type", [Value.ofString "application/json; charset=utf-8"])
            ]
            Data = StringResponseContent json
        }
        Request = Request.get [] (Uri.relative "any")
    }

    Response.parse<CustomerResponse> deserializers response
    |> Assert.errorWith (fun error ->
        match error with
        | DeserializeException (actualResponse, ex) ->
            Assert.Equal(response, actualResponse)
            Assert.IsType<InvalidOperationException>(ex)
        | _ -> failwithf "%A" error
    )

[<Fact>]
let ``given an invalid Content-Type when parse then expected error returned`` () =
    let deserializers =
        Map.empty.Add(MediaType.ofString "json", Json.deserialize)
    let response = {
        StatusCode = HttpStatusCode.OK
        Headers = []
        Content = Some {
            Headers = [
                (Key.ofString "Content-Type", [Value.ofString "json"])
            ]
            Data = StringResponseContent "any"
        }
        Request = Request.get [] (Uri.relative "any")
    }

    Response.parse<CustomerResponse> deserializers response
    |> Assert.errorWith (fun error ->
        match error with
        | ParseError (actualResponse, errorText) ->
            Assert.Equal(response, actualResponse)
            Assert.Equal("Unknown media type.", errorText)
        | _ -> failwithf "%A" error
    )
