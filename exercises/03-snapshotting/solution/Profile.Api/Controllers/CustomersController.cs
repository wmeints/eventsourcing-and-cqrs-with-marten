using Marten;
using Marten.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Profile.Api.Application.ReadModels;
using Profile.Api.Domain;
using Profile.Api.Forms;
using Profile.Api.Shared;

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

    [HttpGet("{customerId:guid}")]
    public async Task<IActionResult> GetCustomerDetails(Guid customerId)
    {
        var result = await _documentSession
            .Query<CustomerInfo>()
            .FirstOrDefaultAsync(x => x.Id == customerId);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("")]
    public async Task<IActionResult> GetCustomers(int pageIndex)
    {
        var results = await _documentSession
            .Query<CustomerInfo>()
            .ToPagedListAsync(pageIndex + 1, 20);

        return Ok(new PagedResult<CustomerInfo>(results,
            (int)results.PageNumber - 1, (int)results.PageSize,
            results.TotalItemCount));
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
        _documentSession.Store(customer); // Store a snapshot for the customer.

        await _documentSession.SaveChangesAsync();

        return Accepted();
    }

    [HttpPost("{customerId:guid}/subscribe")]
    public async Task<IActionResult> StartSubscription(Guid customerId)
    {
        var customer = await _documentSession.Query<Customer>().SingleOrDefaultAsync(x => x.Id == customerId);

        if (customer is { })
        {
            // Restore from the version of the customer snapshot we retrieved.
            customer = await _documentSession.Events.AggregateStreamAsync(
                customerId,
                version: customer.Version,
                state: customer);
        }
        else
        {
            // Perform a regular restore if we haven't stored a snapshot yet.
            customer = await _documentSession.Events.AggregateStreamAsync<Customer>(customerId);
        }

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
