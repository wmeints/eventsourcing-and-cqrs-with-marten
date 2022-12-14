using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Profile.Api.Domain;
using Profile.Api.Forms;

namespace Profile.Api.Controllers;

[ApiController]
[Route("/customers")]
public class CustomersController : ControllerBase
{
    private readonly IDocumentSession _documentSession;

    public CustomersController(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    [HttpPost("")]
    public async Task<IActionResult> RegisterCustomer(RegisterCustomerForm form)
    {
        var customerId = Guid.NewGuid();

        var customer = new Customer(
            customerId,
            form.FirstName,
            form.LastName,
            form.InvoiceAddress,
            form.ShippingAddress,
            form.EmailAddress);

        _documentSession.Events.StartStream<Customer>(customerId, customer.PendingDomainEvents);
        await _documentSession.SaveChangesAsync();

        return Accepted();
    }

    [HttpPost("{customerId:guid}/unsubscribe")]
    public async Task<IActionResult> CancelSubscription(Guid customerId)
    {
        var customer = await _documentSession.Events.AggregateStreamAsync<Customer>(customerId);

        if (customer == null)
        {
            return NotFound();
        }

        customer.Unsubscribe();

        _documentSession.Events.Append(customerId, customer.PendingDomainEvents);
        await _documentSession.SaveChangesAsync();

        return Accepted();
    }

    [HttpPost("{customerId:guid}/subscribe")]
    public async Task<IActionResult> StartSubscription(Guid customerId)
    {
        var customer = await _documentSession.Events.AggregateStreamAsync<Customer>(customerId);

        if (customer == null)
        {
            return NotFound();
        }

        customer.Subscribe();

        _documentSession.Events.Append(customer.Id, customer.PendingDomainEvents);
        await _documentSession.SaveChangesAsync();

        return Accepted();
    }
}