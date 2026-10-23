namespace MxHttp

open System
open System.Text.Json
open Xunit

module Json =

    let serialize object =
        JsonSerializer.Serialize(object)

    let deserialize<'t> (json: string) =
        JsonSerializer.Deserialize<'t>(json)

module Assert =

    let ok = function
        | Ok _ -> ()
        | Error error -> failwithf "%A" error

    let okWith f = function
        | Ok response -> f response
        | Error error -> failwithf "%A" error

    let errorWith f = function
        | Ok response -> failwithf "%A" response
        | Error error -> f error

    let errorWithNonSuccessHttpStatusCode f = function
        | Ok value -> failwithf "%A" value
        | Error (NonSuccessHttpStatusCode response) -> f response
        | Error error -> failwithf "%A" error

    let errorWithResponseContentException f = function
        | Ok value -> failwithf "%A" value
        | Error (ResponseContentException (request, responseMessage, ex)) -> f ex
        | Error error -> failwithf "%A" error

    let requestException<'t> = function
        | Ok (value: Response) -> failwithf "%A" value
        | Error (RequestException (request, ex)) -> Assert.IsType<'t>(ex)
        | Error error -> failwithf "%A" error

    let sameRequest (request: Request) (httpRequestMessage: System.Net.Http.HttpRequestMessage) =
        Assert.Equal(request.Method, httpRequestMessage.Method)
        Assert.Equal(Uri.toSystemUri request.ResourceUri, httpRequestMessage.RequestUri)

module Task =

    let fromResult result =
        System.Threading.Tasks.Task.FromResult(result)

    let result (task: System.Threading.Tasks.Task<'a>) = task.GetAwaiter().GetResult()

module Async =

    let run cancellationToken computation =
        Async.RunSynchronously(computation, cancellationToken = cancellationToken)

type CustomerRequest = {
    Name: string
}

type CustomerResponse = {
    Id: Guid
    Name: string
}

type ErrorMessage = {
    ErrorMessage: string
}
