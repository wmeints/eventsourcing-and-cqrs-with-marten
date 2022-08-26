using Profile.Api.Domain;
using Profile.Api.Domain.Events;

namespace Profile.Api.Tests;

public class CustomerTests
{
    [Fact]
    public void CanRegisterCustomer()
    {
        var customerId = Guid.NewGuid();
        var shippingAddress = new Address("Street", "1", "ZipCode", "City");
        var invoiceAddress = new Address("Street", "1", "ZipCode", "City");

        var customer = new Customer(customerId, "Willem", "Meints",
            invoiceAddress,
            shippingAddress,
            "test@domain.org");

        Assert.NotNull(customer);
        Assert.Single(customer.PendingDomainEvents.Where(x => x is CustomerRegistered));
        Assert.Equal(customerId, customer.Id);
        Assert.Equal("Willem", customer.FirstName);
        Assert.Equal("Meints", customer.LastName);
        Assert.Equal(shippingAddress, customer.ShippingAddress);
        Assert.Equal(invoiceAddress, customer.InvoiceAddress);
        Assert.Equal("test@domain.org", customer.EmailAddress);
        Assert.NotNull(customer.Subscription);
    }

    [Fact]
    public void CanCancelSubscription()
    {
        var customerId = Guid.NewGuid();
        var shippingAddress = new Address("Street", "1", "ZipCode", "City");
        var invoiceAddress = new Address("Street", "1", "ZipCode", "City");

        var customer = new Customer(customerId, "Willem", "Meints",
            invoiceAddress,
            shippingAddress,
            "test@domain.org");

        customer.Unsubscribe();

        Assert.Single(customer.PendingDomainEvents.Where(x => x is SubscriptionCanceled));
        Assert.NotNull(customer.Subscription);
        Assert.NotNull(customer.Subscription!.EndDate);
    }

    [Fact]
    public void CanResubscribe()
    {
        var customerId = Guid.NewGuid();
        var shippingAddress = new Address("Street", "1", "ZipCode", "City");
        var invoiceAddress = new Address("Street", "1", "ZipCode", "City");

        var customer = new Customer(customerId, "Willem", "Meints",
            invoiceAddress,
            shippingAddress,
            "test@domain.org");

        customer.Unsubscribe();
        customer.Resubscribe();
        
        Assert.Single(customer.PendingDomainEvents.Where(x => x is SubscriptionStarted));
        Assert.NotNull(customer.Subscription);
        Assert.Null(customer.Subscription!.EndDate);
    }
}