module MxHttp.HttpTaskTests

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
    responseMessage.Content.Headers.ContentLength <- json.Length
    responseMessage

[<Fact>]
let ``given a Request without Content when send then returns correct Response`` () =
    use cancellationTokenSource = new CancellationTokenSource()
    let request = Request.delete [] (Uri.absolute "http://localhost/api/customers/123456789")
    let httpSend cancellationToken httpRequestMessage =
        Assert.Equal(cancellationTokenSource.Token, cancellationToken)
        Assert.sameRequest request httpRequestMessage

        cancellationToken.ThrowIfCancellationRequested()

        responseMessage HttpStatusCode.NoContent
        |> Task.fromResult

    request
    |> Http.Task.send httpSend cancellationTokenSource.Token
    |> Task.result
    |> Assert.okWith (fun response ->
        Assert.Equal(request, response.Request)
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode)
        Assert.Equal<Header list>([], response.Headers)
        Assert.Equal(None, response.Content)
    )

[<Fact>]
let ``given a Request with Content when send then returns correct Response`` () =
    use cancellationTokenSource = new CancellationTokenSource()
    let request =
        { Name = "Any" }
        |> RequestContent.string Json.serialize MediaType.applicationJson
        |> Request.post [Header.acceptJson] (Uri.absolute "http://localhost/api/customers")
    let expectedResponseObject = { Id = Guid.NewGuid(); Name = "added" }
    let expectedJson = Json.serialize expectedResponseObject
    let expectedResponseContent = {
        Headers = [
            Key.ofString "Content-Type", [Value.ofString "application/json; charset=utf-8"]
            Key.ofString "Content-Length", [Value.ofString (string expectedJson.Length)]
        ]
        Data = StringResponseContent expectedJson
    }
    let httpSend cancellationToken httpRequestMessage =
        Assert.Equal(cancellationTokenSource.Token, cancellationToken)
        Assert.sameRequest request httpRequestMessage

        cancellationToken.ThrowIfCancellationRequested()

        responseMessage HttpStatusCode.OK
        |> withJsonStringContent expectedResponseObject
        |> Task.fromResult

    request
    |> Http.Task.send httpSend cancellationTokenSource.Token
    |> Task.result
    |> Assert.okWith (fun response ->
        Assert.Equal(request, response.Request)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode)
        Assert.Equal<Header list>([], response.Headers)
        Assert.Equal(Some expectedResponseContent, response.Content)
    )

[<Fact>]
let ``given a Request when send throws TaskCanceledException then returns correct Error`` () =
    use cancellationTokenSource = new CancellationTokenSource()
    let request = Request.delete [] (Uri.absolute "http://localhost/api/customers/123456789")
    let httpSend cancellationToken httpRequestMessage =
        Assert.Equal(cancellationTokenSource.Token, cancellationToken)
        Assert.sameRequest request httpRequestMessage

        cancellationToken.ThrowIfCancellationRequested()

        raise (TaskCanceledException())

    request
    |> Http.Task.send httpSend cancellationTokenSource.Token
    |> Task.result
    |> Assert.requestException<TaskCanceledException>

[<Fact>]
let ``given a Request when send is canceled then returns a correct Error`` () =
    use cancellationTokenSource = new System.Threading.CancellationTokenSource()
    let request = Request.delete [] (Uri.absolute "http://localhost/api/customers/123456789")
    let httpSend cancellationToken httpRequestMessage =
        Assert.Equal(cancellationTokenSource.Token, cancellationToken)
        Assert.sameRequest request httpRequestMessage

        cancellationTokenSource.Cancel()

        cancellationToken.ThrowIfCancellationRequested()

        responseMessage HttpStatusCode.NoContent
        |> Task.fromResult

    request
    |> Http.Task.send httpSend cancellationTokenSource.Token 
    |> Task.result 
    |> Assert.requestException<OperationCanceledException>
