namespace MxHttp

open System.IO
open System.Net
open System.Net.Http
open System.Threading.Tasks

module HttpResponseMessage =

    val statusCode: HttpResponseMessage -> HttpStatusCode

    val headers: HttpResponseMessage -> Header list

    val httpContent: HttpResponseMessage -> HttpContent
