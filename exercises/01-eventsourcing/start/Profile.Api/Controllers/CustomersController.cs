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
        
        //TODO: Implement method
        
        return Accepted();
    }

    [HttpPost("{customerId:guid}/unsubscribe")]
    public async Task<IActionResult> CancelSubscription(Guid customerId)
    {
        //TODO: Implement method
        return Accepted();
    }

    [HttpPost("{customerId:guid}/subscribe")]
    public async Task<IActionResult> StartSubscription(Guid customerId)
    {
        //TODO: Implement method
        return Accepted();
    }
}