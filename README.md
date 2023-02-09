# MxHttp

A functional implementation of HTTP client built on top of System.Net.Http.

The basic goal for this library is to simplify HTTP related code in F#.

Functional programming uses immutable data structures and transformation of objects using functions.

The library contains immutable types Request and Response. Also, the library does not throw exceptions and it catches the exceptions thown by the infrastructure and wraps them into Failure type's cases.

The HTTP request can be composed from series of transformations:
- object -> RequestContent
- RequestContent -> Request
- Request -> HttpRequestMessage
- HttpRequestMessage -> HttpResponseMessage
- HttpResponseMessage -> Response
- Response -> object

With Request type you can simplify retries using Polly. I guess you know the problem that when the HttpRequestMessage has already been sent then you cannot send it again.

When you wrap retry logic around Request -> HttpRequestMessage -> HttpResponseMessage transformations then on each retry a new HttpRequestMessage will be created.