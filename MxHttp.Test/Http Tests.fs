module MxHttp.HttpTests

open System
open System.Net
open System.Net.Http
open System.Text
open System.Threading
open System.Threading.Tasks
open Xunit

let responseMessage statusCode =
    new HttpResponseMessage(statusCode)

let withJsonStringContent content (responseMessage: HttpResponseMessage) =
    let json = Json.serialize content
    let mediaType = MediaType.toString MediaType.applicationJson
    responseMessage.Content <- new StringContent(json, Encoding.UTF8, mediaType)
    responseMessage

let taskFromResponseMessage (responseMessage: HttpResponseMessage) =
    Task.FromResult(responseMessage)

[<Fact>]
let ``given an async computation expression when send delete then returns correct Response`` () =
    let request = Request.delete [] (Uri.absolute "http://localhost/api/customers/123456789")
    let send ctoken req =
        responseMessage HttpStatusCode.NoContent
        |> taskFromResponseMessage

    async {
        return! Http.Async.send send request
    }
    |> Async.RunSynchronously
    |> Assert.okWith (fun response ->
        Assert.Equal(request, response.Request)
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode)
        Assert.Equal<Header list>([], response.Headers)
        Assert.Equal(None, response.Content)
    )

[<Fact>]
let ``given an async computation expression when send post then returns correct Response`` () =
    let request = 
        { Name = "Any" }
        |> Content.string Json.serialize MediaType.applicationJson
        |> Request.post [Header.acceptJson] (Uri.absolute "http://localhost/api/customers")
    let responseObject = { Id = Guid.NewGuid(); Name = "added" }
    let responseJson = Json.serialize responseObject
    let responseJsonLength = responseJson.Length
    let responseContent = {
        Headers = [
            Key.ofString "Content-Type", [Value.ofString "application/json; charset=utf-8"]
            Key.ofString "Content-Length", [Value.ofString (responseJsonLength.ToString())]
        ]
        Data = StringResponseContent responseJson
    }
    let send ctoken req =
        responseMessage HttpStatusCode.OK
        |> withJsonStringContent responseObject
        |> taskFromResponseMessage

    async {
        return! Http.Async.send send request
    }
    |> Async.RunSynchronously
    |> Assert.okWith (fun response ->
        Assert.Equal(request, response.Request)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode)
        Assert.Equal<Header list>([], response.Headers)
        Assert.Equal(Some responseContent, response.Content)
    )

[<Fact>]
let ``given an async computation expression when send throws TaskCanceledException then returns correct Error`` () =
    let request = Request.delete [] (Uri.absolute "http://localhost/api/customers/123456789")
    let send req =
        raise (TaskCanceledException())

    async {
        return! Http.Async.send send request
    }
    |> Async.RunSynchronously
    |> Assert.errorWith (fun error ->
        match error with
        | RequestException (req, ex) ->
            Assert.Equal(request, req)
            Assert.IsType<TaskCanceledException>(ex)
        | _ -> failwith "?"
    )

[<Fact>]
let ``given an async computation expression when send response is an HTTP error then returns correct Error`` () =
    let request = Request.delete [] (Uri.absolute "http://localhost/api/customers/123456789")
    let responseObject = { ErrorMessage = "cannot delete the customer because the customer has invoices" }
    let responseJson = Json.serialize responseObject
    let responseJsonLength = responseJson.Length
    let responseContent = {
        Headers = [
            Key.ofString "Content-Type", [Value.ofString "application/json; charset=utf-8"]
            Key.ofString "Content-Length", [Value.ofString (responseJsonLength.ToString())]
        ]
        Data = StringResponseContent responseJson
    }
    let send ctoken req =
        responseMessage HttpStatusCode.BadRequest
        |> withJsonStringContent responseObject
        |> taskFromResponseMessage

    async {
        return! Http.Async.send send request
    }
    |> Async.RunSynchronously
    |> Assert.errorWith (fun error ->
        match error with
        | NonSuccessHttpStatusCode response ->
            Assert.Equal(request, response.Request)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
            Assert.Equal<Header list>([], response.Headers)
            Assert.Equal(Some responseContent, response.Content)
        | _ -> failwith "?"
    )

// Test cases:

    // with short timeout and longer execution time
        // -> should respond with error result

    // with long timeout and cancelled using cancellation token
        // -> should respond with error result

    // with http server returning 500
        // -> should respond with error result

//[<Fact>]
//let ``given a synchronous pipeline when executed then returns ok`` () =
//    { Name = "Any" }
//    |> Request.Content.string Json.serialize MediaType.applicationJson
//    |> Request.post [Header.acceptJson] (Uri.absolute "http://localhost/api/customers")
//    |> Http.send sendRequest
//    |> Result.bind (Response.parse<Customer> [MediaType.applicationJson, Json.deserialize])
//    |> Assert.ok

//[<Fact>]
//let ``given an async computation expression when executed then returns ok`` () =
//    async {
//        let request =
//            { Name = "Any" }
//            |> Request.Content.string Json.serialize MediaType.applicationJson
//            |> Request.post [Header.acceptJson] (Uri.absolute "http://localhost/api/customers")

//        let! result = Http.Async.send sendRequest request

//        return Result.bind (Response.parse<Customer> [MediaType.applicationJson, Json.deserialize]) result
//    }
//    |> Async.RunSynchronously
//    |> Assert.ok

//[<Fact>]
//let ``given a task computation expression when executed then returns ok`` () =
//    task {
//        let request =
//            { Name = "Any" }
//            |> Request.Content.string Json.serialize MediaType.applicationJson
//            |> Request.post [Header.acceptJson] (Uri.absolute "http://localhost/api/customers")

//        let! result = Http.Task.send sendRequest request

//        return Result.bind (Response.parse<Customer> [MediaType.applicationJson, Json.deserialize]) result
//    }
//    |> Task.result
//    |> Assert.ok

//[<Fact>]
//let ``given a backgroundTask computation expression when executed then returns ok`` () =
//    backgroundTask {
//        let request =
//            { Name = "Any" }
//            |> Request.Content.string Json.serialize MediaType.applicationJson
//            |> Request.post [Header.acceptJson] (Uri.absolute "http://localhost/api/customers")

//        let! result = Http.BackgroundTask.send sendRequest request

//        return Result.bind (Response.parse<Customer> [MediaType.applicationJson, Json.deserialize]) result
//    }
//    |> Task.result
//    |> Assert.ok
