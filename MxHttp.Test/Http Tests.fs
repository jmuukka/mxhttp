module MxHttp.HttpTests

open System
open System.Net.Http
open System.Text.Json
open System.Threading.Tasks
open Xunit

module Json =

    let serialize object =
        JsonSerializer.Serialize(object)

    let deserialize<'t> (json : string) =
        JsonSerializer.Deserialize<'t>(json)

module Assert =

    let ok = function
        | Ok _ -> ()
        | Error error -> failwithf "%A" error

module Task =

    let result (task : Task<_>) =
        let awaiter = task.GetAwaiter()
        awaiter.GetResult()

type Customer = {
    Name : string
}

let sendRequest (request : HttpRequestMessage) : Task<HttpResponseMessage> =
    let response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
    response.Content <- request.Content
    Task.FromResult(response)

[<Fact>]
let ``given a synchronous pipeline when executed then returns ok`` () =
    { Name = "Any" }
    |> Request.Content.string Json.serialize MediaType.applicationJson
    |> Request.post [Header.acceptJson] (Uri.absolute "http://localhost/api/customers")
    |> Http.send sendRequest
    |> Result.bind (Response.parse<Customer> [MediaType.applicationJson, Json.deserialize])
    |> Assert.ok

[<Fact>]
let ``given an async computation expression when executed then returns ok`` () =
    async {
        let request =
            { Name = "Any" }
            |> Request.Content.string Json.serialize MediaType.applicationJson
            |> Request.post [Header.acceptJson] (Uri.absolute "http://localhost/api/customers")

        let! result = Http.Async.send sendRequest request

        return Result.bind (Response.parse<Customer> [MediaType.applicationJson, Json.deserialize]) result
    }
    |> Async.RunSynchronously
    |> Assert.ok

[<Fact>]
let ``given a task computation expression when executed then returns ok`` () =
    task {
        let request =
            { Name = "Any" }
            |> Request.Content.string Json.serialize MediaType.applicationJson
            |> Request.post [Header.acceptJson] (Uri.absolute "http://localhost/api/customers")

        let! result = Http.Task.send sendRequest request

        return Result.bind (Response.parse<Customer> [MediaType.applicationJson, Json.deserialize]) result
    }
    |> Task.result
    |> Assert.ok

[<Fact>]
let ``given a backgroundTask computation expression when executed then returns ok`` () =
    backgroundTask {
        let request =
            { Name = "Any" }
            |> Request.Content.string Json.serialize MediaType.applicationJson
            |> Request.post [Header.acceptJson] (Uri.absolute "http://localhost/api/customers")

        let! result = Http.BackgroundTask.send sendRequest request

        return Result.bind (Response.parse<Customer> [MediaType.applicationJson, Json.deserialize]) result
    }
    |> Task.result
    |> Assert.ok
