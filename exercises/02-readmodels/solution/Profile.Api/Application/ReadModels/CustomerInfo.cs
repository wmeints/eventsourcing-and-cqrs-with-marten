using Profile.Api.Domain;

namespace Profile.Api.Application.ReadModels;

public record CustomerInfo(
    Guid Id,
    string FirstName,
    string LastName,
    Address InvoiceAddress,
    Address ShippingAddress,
    bool Subscribed);
