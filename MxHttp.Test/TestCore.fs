namespace MxHttp

//open System
open System.IO
open System.Text.Json
open System.Threading.Tasks
open Xunit

module Json =

    let serialize<'t> object =
        JsonSerializer.Serialize<'t>(object)

    let deserialize<'t> (json: string) =
        JsonSerializer.Deserialize<'t>(json)

module Result =

    let ok = function
        | Ok value -> value
        | Error error -> failwithf "%A" error

    let eitherBind onOk onError = function
        | Ok value -> onOk value
        | Error error -> onError error

module Task =

    let result (task: System.Threading.Tasks.Task<'a>) = task.GetAwaiter().GetResult()

    let fromResult result =
        System.Threading.Tasks.Task.FromResult(result)

    let retn value =
        Task.FromResult value

    let bind binder input =
        backgroundTask {
            let! value = input
            return! binder value
        }

    let map mapper input =
        bind (mapper >> retn) input

module TaskResult =

    let error err = Task.retn (Error err)

    let bind f taskResult =
        Task.bind (Result.eitherBind f error) taskResult

    let mapError mapper input =
        Task.map (Result.mapError mapper) input

module Async =

    let run computation =
        Async.RunSynchronously(computation)

    let runWithCancellationToken cancellationToken computation =
        Async.RunSynchronously(computation, cancellationToken = cancellationToken)

module Assert =

//    let ok = function
//        | Ok _ -> ()
//        | Error error -> failwithf "%A" error

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

    let requestException<'t> = function
        | Ok (value: Response) -> failwithf "%A" value
        | Error (RequestException (request, ex)) -> Assert.IsType<'t>(ex)
        | Error error -> failwithf "%A" error

    let sameRequest (request: Request) (httpRequestMessage: System.Net.Http.HttpRequestMessage) =
        Assert.Equal(request.Method, httpRequestMessage.Method)
        Assert.Equal(Uri.toSystemUri request.ResourceUri, httpRequestMessage.RequestUri)

module Http =

    module Task =

        let httpSendReturns httpResponse : HttpSendTask =
            fun cancellationToken httpRequestMessage ->
                cancellationToken.ThrowIfCancellationRequested()
                Task.fromResult httpResponse

    module Async =

        let httpSendReturns httpResponse : HttpSendAsync =
            fun httpRequestMessage ->
                async {
                    return httpResponse
                }

    let readStreamAsByteArray bufferSize (stream: Stream) =
        let buffer = Array.init<byte> bufferSize (fun x -> 0uy)
        let length = stream.Read(buffer)
        Array.take length buffer

module JsonHttpContent =

    let ofObject<'t> object =
        Json.serialize<'t> object
        |> HttpContent.ofString MediaType.applicationJson
