using Marten.Events.Aggregation;
using Profile.Api.Application.ReadModels;
using Profile.Api.Domain.Events;

namespace Profile.Api.Application.Projections;

public class CustomerInfoProjection: SingleStreamAggregation<CustomerInfo>
{
public static CustomerInfo Create(CustomerRegistered evt) => new CustomerInfo(
    evt.Id, evt.FirstName, evt.LastName,
    evt.InvoiceAddress, evt.ShippingAddress, true);

public CustomerInfo Apply(SubscriptionCanceled evt, CustomerInfo current) => current with
{
    Subscribed = false
};

public CustomerInfo Apply(SubscriptionStarted evt, CustomerInfo current) => current with
{
    Subscribed = true  
};
}