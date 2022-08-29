using System.Diagnostics.CodeAnalysis;
using Profile.Api.Domain;

namespace Profile.Api.Forms;

public class RegisterCustomerForm
{
    public string FirstName { get; set; } = "";
    
    public string LastName { get; set; }= "";
    
    public string EmailAddress { get; set; }= "";
    
    [NotNull]
    public Address? InvoiceAddress { get; set; }
    
    [NotNull]
    public Address? ShippingAddress { get; set; }
}