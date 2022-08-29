namespace Profile.Api.Domain.Events;

public record CustomerRegistered(
    Guid Id,
    string FirstName, 
    string LastName,
    Address InvoiceAddress,
    Address ShippingAddress,
    string EmailAddress);