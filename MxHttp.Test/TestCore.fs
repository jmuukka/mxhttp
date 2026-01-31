namespace MxHttp

open System
open System.Text.Json

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
