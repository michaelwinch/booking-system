module LambdaHandlers

open System
open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open Amazon.Lambda.Serialization.SystemTextJson

[<LambdaSerializer(typeof<DefaultLambdaJsonSerializer>)>]
let createBooking (event: APIGatewayProxyRequest) = // todo: add the nuget package for the lambda trigger event type
    let result =
        event.Body
        |> Newtonsoft.Json.JsonConvert.DeserializeObject<Workflows.RequestDTOs.CreateBooking>
        |> Workflows.createBooking
      
    match result with
    | Ok _ -> APIGatewayProxyResponse(StatusCode = 200, Body = "Success")
    | Error x -> APIGatewayProxyResponse(StatusCode = 500, Body = x)
    
[<LambdaSerializer(typeof<DefaultLambdaJsonSerializer>)>]
let listBookings (event: APIGatewayProxyRequest) =
    let result =
        event.Body
        |> Guid.Parse
        |> Workflows.listBookings
      
    match result with
    | Ok bookings ->
        bookings
        |> Newtonsoft.Json.JsonConvert.SerializeObject
        |> fun x -> APIGatewayProxyResponse(StatusCode = 200, Body = x)
    | Error x -> APIGatewayProxyResponse(StatusCode = 500, Body = x)