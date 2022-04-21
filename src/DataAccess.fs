module DataAccess

open System

type BookingDTO =
    {
        Id: Guid
        CustomerId: Guid
        Item: string
        Status: string
        StartDate: DateTime
        EndDate: DateTime
    }
    
module BookingDTO =
    let toDomain dto : Booking =
        {
            Id = BookingId dto.Id
            CustomerId = CustomerId dto.CustomerId
            Item = dto.Item
            Status = BookingStatus.ofString dto.Status
            StartDate = dto.StartDate
            EndDate = dto.EndDate
        }
        
    let ofDomain (domain: Booking) : BookingDTO =
        {
            Id = BookingId.unwrap domain.Id
            CustomerId = CustomerId.unwrap domain.CustomerId
            Item = domain.Item
            Status = BookingStatus.toString domain.Status
            StartDate = domain.StartDate
            EndDate = domain.StartDate
        }