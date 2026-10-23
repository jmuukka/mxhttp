namespace MxHttp

open System.Threading.Tasks

module private Result =

    let eitherBind onOk onError = function
        | Ok value -> onOk value
        | Error error -> onError error

module private Task =

    let retn value =
        Task.FromResult value

    let bind binder input =
        backgroundTask {
            let! value = input
            return! binder value
        }

    let map mapper input =
        bind (mapper >> retn) input

module private TaskResult =

    let error err = Task.retn (Error err)

    let bind f taskResult =
        Task.bind (Result.eitherBind f error) taskResult

    // let map f taskResult = Task.map (Result.map f) taskResult

    let mapError mapper input =
        Task.map (Result.mapError mapper) input

module private Async =

    let retn value =
        async.Return value

    let bind binder input =
        async.Bind(input, binder)

    let map mapper input =
        bind (mapper >> retn) input

module private AsyncResult =

    let error err =
        Async.retn (Error err)

    let bind binder input =
        Async.bind (Result.eitherBind binder error) input

    let mapError mapper input =
        Async.map (Result.mapError mapper) input
