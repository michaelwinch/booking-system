module Workflows

open System
open DataAccess

[<AutoOpen>]
module RequestDTOs =
    type CreateBooking =
        {
            CustomerId: Guid
            Item: string
            StartDate: DateTime
            EndDate: DateTime
        }
    
    module CreateBooking =
        let toDomain id dto : Booking =
            {
                Id = BookingId id
                CustomerId = CustomerId dto.CustomerId
                Item = dto.Item
                Status = BookingStatus.Requested
                StartDate = dto.StartDate
                EndDate = dto.EndDate
            }


let createBooking (booking: CreateBooking) =
    result {
        let booking =
            booking
            |> CreateBooking.toDomain (Guid.NewGuid())
        
        do!
            booking
            |> BookingDTO.ofDomain
            |> Aws.DB.put
        
        do! Aws.Messages.publish (Message.BookingCreated booking)
    }
    
let listBookings customerId =
    CustomerId customerId
    |> Aws.DB.query