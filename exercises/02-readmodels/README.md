# Exercise 2 - Building read models

In the previous exercise we build a domain model that's event-sourced. We've established that while the domain model
useful in that we can now maintain state properly and rewind events. However, the current domain model is kind of
hard to work with from a read perspective. Every time we want to read information we need to replay events.

We'll need to create one or more representations of the information from the event-sourced entity for reading. From
here on we call the aggregate a write model and the representation of the aggregate a read model.

To create a read model, we'll need to use a processor that turns the events into a representation. The processor we call
a projection. You can have multiple projections in your service. How many projections you have depends on the sort of
queries that you want to process in your service.

In this exercise we're going to build a representation of the profile which allows us to display some basic information
of a user in a frontend. We'll use what's called a single stream aggregation to make this representation. We'll perform
the following tasks:

* Add a new projection to the service
* Register the projection with Marten
* Use the read model from the service

Let's get started by building a read model and a new projection.

## Step 1 - Add a new projection to the service

In this first step, we'll create a read model called `CustomerInfo`. You can find the class in the folder
`exercises/02-readmodels/start/Profile.Api/Application/ReadModels/CustomerInfo.cs`. It looks like this:

```csharp
using Profile.Api.Domain;

namespace Profile.Api.Application.ReadModels;

public record CustomerInfo(
    Guid Id,
    string FirstName,
    string LastName,
    Address InvoiceAddress,
    Address ShippingAddress,
    bool Subscribed);
```

Entities of type `CustomerInfo` will have the same value for ID as we used for the related event stream in the previous
example. We're also storing almost identical information about the customer. The difference with the aggregate is that
we only store a flag whether the customer is subscribed instead of full subscription information.

To fill the read model with useful information, we're going to create a new projection.
Create a new file in the folder `exercises/02-readmodels/start/Profile.Api/Application/Projections/` with the name
`CustomerInfoProjection.cs`. Replace the content of the new file with the following code:

```csharp
using Marten.Events.Aggregation;
using Profile.Api.Application.ReadModels;
using Profile.Api.Domain.Events;

namespace Profile.Api.Application.Projections;

public class CustomerInfoProjection: SingleStreamAggregation<CustomerInfo>
{

}
```

We're deriving the new projection class from the `SingleStreamAggregation` type. By building this kind of projection
we're telling Marten that we want to roll up events related to a single stream into a single representation.

Let's add the first event projection function to the new projection type. Add the following method to the class:

```csharp
public static CustomerInfo Create(CustomerRegistered evt) => new CustomerInfo(
    evt.Id, evt.FirstName, evt.LastName,
    evt.InvoiceAddress, evt.ShippingAddress, true);
```

To create a new instance of the read model, we'll have to write a `Create` method that accepts the event that causes
the insert operation to happen in the database. The `Create` method must be static and it should return the read model
instance we want to insert.

In our project we'll take the `CustomerRegistered` event and convert it into the `CustomerInfo` instance. Since we
automatically subscribe when a customer registers, we can set the `Subscribed` property to `true`.

Now that we have a read model, we need to update it when a customer cancels their subscription. Add the following
code to the projection class:

```csharp
public CustomerInfo Apply(SubscriptionCanceled evt, CustomerInfo current) => current with
{
    Subscribed = false
};
```

This method updates the read model with the new subscription state. Methods that update read models must be named
`Apply` otherwise `Marten` isn't going to find them. The apply methods don't need to be static, but they can be if you
want a tiny bit extra performance.

We use the `with` syntax in C# to return a copy the original customer info record with a new value for
the `Subscribed` flag. Marten will update the database with the new customer information.

The final projection we need is to update the subscription status when a customer resubscribes to the service.
Add the following code to the projection to implement the projection:

```csharp
public CustomerInfo Apply(SubscriptionStarted evt, CustomerInfo current) => current with
{
    Subscribed = true  
};
```

This projection uses the same syntax as the previous projection method we created.

Now that we have the projection made, let's wire it up to Marten so that events get projected to the new read model.

## Step 2 - Register the projection with Marten

Right now, the projection does nothing because Marten doesn't know about it. We need to modify the code in `Program.cs`
to include the projection. Open the file `exercises/02-readmodels/Profile.Api/Program.cs` and modify the line that calls
`builder.Services.AddMarten` to include the projection. Add a new line to the lambda that configures Marten:

```csharp
marten.Projections.Add<CustomerInfoProjection>();
```

After modifying the Marten configuration, the code should look like this:

```csharp
builder.Services.AddMarten(marten =>
{
    marten.Connection(builder.Configuration.GetConnectionString("DefaultDatabase"));
    marten.AutoCreateSchemaObjects = AutoCreate.CreateOnly;

    marten.Projections.Add<CustomerInfoProjection>();
});
```

The last step in this exercise is to use the new read model in the API.

## Step 3 - Use the read model from the service

To use the read model, we can query the `CustomerInfo` type as a regular document in Marten. Marten allows you to query
documents by calling the `Query<T>` method on the `IDocumentSession` that we've used previously to access event store.

Let's add a method to query a single customer from the API.

Add a new method to controller class in the file `exercises/02-readmodels/Profile.Api/Controllers/CustomersController.cs`
using the following code:

```csharp
[HttpGet("{customerId:guid}")]
public async Task<IActionResult> GetCustomerDetails(Guid customerId) 
{
    var result = await _documentSession.Query<CustomerInfo>().FirstOrDefaultAsync(x=>x.Id == customerId);
    
    if(result == null) 
    {
        return NotFound();
    }

    return Ok(result);
}
```

To complete the query part of the application, let's also add a method to get a list of customers. We're going to use
paging to limit the number of customers returned by the API. Add the following code to the controller file:

```csharp
[HttpGet("")]
public async Task<IActionResult> GetCustomers(int pageIndex)
{
    var results = await _documentSession
        .Query<CustomerInfo>()
        .ToPagedListAsync(pageIndex, 20);

    return Ok(new PagedResult<CustomerInfo>(results, 
        (int)results.PageNumber, (int)results.PageSize,
        results.TotalItemCount));
}
```

First, we're querying the `CustomerInfo` read model. Instead of just returning a list, we're going to use a special
operation offered by Marten, called `ToPagedListAsync`. By using the paging operation we're only getting a limited
set of customers based on the page index we choose to send to the API.

We're returning a new `PagedResult<T>` instance with the information we retrieved from the database.

## Testing your work

You can test the API using [the REST client addon][REST_ADDON] for Visual Studio Code. We've included a few sample
files in the [requests](../../requests/) folder.

Make sure you've started the postgres database server as per [the instructions](../../README.md#preparing-your-machine)
in the root of the repository.

## Summary

In this exercise we've added a read model to the application and wrote methods to query the read model. 
In the last exercise we're going to add an application layer to implement CQRS using command handlers and query
handlers.

[Next exercise](../03-cqrs/README.md)

[REST_ADDON]: https://marketplace.visualstudio.com/items?itemName=humao.rest-client