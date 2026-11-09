namespace MxHttp

open System

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
