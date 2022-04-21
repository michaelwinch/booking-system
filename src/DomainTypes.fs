[<AutoOpen>]
module DomainTypes
open System

type BookingId = BookingId of Guid
module BookingId =
    let unwrap = function BookingId x -> x

type CustomerId = CustomerId of Guid
module CustomerId =
    let unwrap = function CustomerId x -> x

type BookingStatus =
    | Requested
    | Confirmed
    | Cancelled
    | Completed

module BookingStatus =
    let ofString =
        function
        | "Requested" -> Requested
        | "Confirmed" -> Confirmed
        | "Cancelled" -> Cancelled
        | "Completed" -> Completed
        | x -> failwithf "could not understand booking status %s" x
        
    let toString =
        function
        | Requested -> "Requested"
        | Confirmed -> "Confirmed"
        | Cancelled -> "Cancelled"
        | Completed -> "Completed"
            

type Booking =
    {
        Id: BookingId
        CustomerId: CustomerId
        Item: string
        Status: BookingStatus
        StartDate: DateTime
        EndDate: DateTime
    }
    
    
type Message =
    | BookingCreated of Booking