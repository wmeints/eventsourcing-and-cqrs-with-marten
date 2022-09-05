using Marten.Linq.SoftDeletes;
using Profile.Api.Domain.Events;
using Profile.Api.Shared;

namespace Profile.Api.Domain;

public class Customer : AggregateRoot
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Address InvoiceAddress { get; private set; }
    public Address ShippingAddress { get; private set; }
    public string EmailAddress { get; private set; }
    public Subscription? Subscription { get; private set; }

    private Customer()
    {
    }

    public Customer(Guid id, string firstName, string lastName, Address invoiceAddress, Address shippingAddress,
        string emailAddress)
    {
        Emit(new CustomerRegistered(id, firstName, lastName, invoiceAddress, shippingAddress, emailAddress));
    }

    public void Unsubscribe()
    {
        Emit(new SubscriptionCanceled(Id, DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    public void Subscribe()
    {
        Emit(new SubscriptionStarted(Id, DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    protected override bool TryApplyDomainEvent(object domainEvent)
    {
        switch (domainEvent)
        {
            case CustomerRegistered customerRegistered:
                Apply(customerRegistered);
                break;
            case SubscriptionCanceled subscriptionCanceled:
                Apply(subscriptionCanceled);
                break;
            case SubscriptionStarted subscriptionStarted:
                Apply(subscriptionStarted);
                break;
            default:
                return false;
        }

        return true;
    }

    private void Apply(SubscriptionStarted subscriptionStarted)
    {
        Subscription = new Subscription(subscriptionStarted.StartDate, null);
        Version++;
    }

    private void Apply(SubscriptionCanceled subscriptionCanceled)
    {
        Subscription = Subscription with { EndDate = subscriptionCanceled.EndDate };
        Version++;
    }

    private void Apply(CustomerRegistered customerRegistered)
    {
        Id = customerRegistered.Id;
        FirstName = customerRegistered.FirstName;
        LastName = customerRegistered.LastName;
        InvoiceAddress = customerRegistered.InvoiceAddress;
        ShippingAddress = customerRegistered.ShippingAddress;
        EmailAddress = customerRegistered.EmailAddress;
        Subscription = new Subscription(DateOnly.FromDateTime(DateTime.UtcNow), null);
        Version++;
    }
}
