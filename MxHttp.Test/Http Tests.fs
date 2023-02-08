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

type Customer = {
    Name : string
}

let sendRequest (request : HttpRequestMessage) : Task<HttpResponseMessage> =
    let response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
    response.Content <- request.Content
    Task.FromResult(response)

[<Fact>]
let ``given a post pipeline when executed then returns ok`` () =
    { Name = "Any" }
    |> Request.Content.string Json.serialize MediaType.applicationJson
    |> Request.post [Header.acceptJson] (Uri.absolute "http://localhost/api/customers")
    |> Http.send sendRequest
    |> Result.bind (Response.parse<Customer> [MediaType.applicationJson, Json.deserialize])
    |> Assert.ok
