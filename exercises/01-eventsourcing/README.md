# Exercise 1 - Implementing event-sourced entities

In this first exercise, we'll lay the foundation for the profile API of the tasty-beans application. We'll perform the
following tasks:

* Implement the domain logic for the API.
* Configure the event store to persist the domain events.
* Connect the domain logic to the REST interface.

Let's get started by implementing the domain logic for the profile API.

## Implement the domain logic for the API

The domain logic of the profile API needs to implement a couple of operations:

* Register customer - Registers customer profile information and starts the subscription.
* Unsubscribe - Cancels the current subscription with a provided end date.
* Resubscribe - Resubscribe the customer to the coffee delivery service.

### Preparing the code base

Before we can start writing domain logic, we need a couple of basic building blocks. We're going to use a base class
called an `AggregateRoot`. This class identifies entities that are the root of an aggregate in the application.

We'll add a couple of valuable operations to the `AggregateRoot` class.

------------------------------------------------------------------------------------------------------------------------

**What is an aggregate**  
The aggregate design pattern is an essential one you'll often find in microservices. It's part
of the Domain Driven Design movement. 

An aggregate is a collection of entities or objects you can treat as a single unit. Operations that modify the state
of one of the entities are executed against the aggregate root. The aggregate root distributes the changes across the
related objects in the application.

Transactions in microservices should apply to one aggregate at a time. You shouldn't change two aggregates in the 
same HTTP request. 

------------------------------------------------------------------------------------------------------------------------

Create a new class in the folder `exercises/01-eventsourcing/start/Profile.Api/Shared` with the name `AggregateRoot.cs`.
Add the following content to the class:

```csharp
namespace Profile.Api.Shared;

public abstract class AggregateRoot
{
    private readonly List<object> _pendingDomainEvents = new();

    public Guid Id { get; protected set; }
    
    public IReadOnlyCollection<object> PendingDomainEvents => _pendingDomainEvents.AsReadOnly();

    protected void Emit(object domainEvent)
    {
        if (TryApplyDomainEvent(domainEvent))
        {
            _pendingDomainEvents.Add(domainEvent);
        }
    }

    protected abstract bool TryApplyDomainEvent(object domainEvent);
}
```

Let's go over each of the parts in this class:

1. First, we create a private list containing any pending domain events that we need to process.
2. After this, we add a property containing a read-only list of domain events to be used by the application code later.
3. Next, we add a protected method called `Emit`. The aggregate uses this method to generate a domain event.
4. Then, we add another protected method `TryApplyDomainEvent`. This method is abstract; we'll implement it later.
   The `Emit` method calls the `TryApplyDomainEvent` to match the event against an event handler which will update the
   aggregate state. When we've successfully applied the domain event, we can add the event to the list of
   generated domain events we'll store later.

Now that we have the basics, we can start building the aggregate.

### Create the Customer aggregate

The profile service has the concept of a Customer. Each customer has a subscription with a start date and a possible
end date. Each customer has a first name, last name, shipping address, and invoice address.

First, create a new file in `exercises/01-eventsourcing/start/Profile.Api/Domain/Address.cs`.
Replace the content of the file with the following code:

```csharp
namespace Profile.Api.Domain;

public record Address(string Street, string BuildingNumber, string ZipCode, string City);
```

Next, add a file `exercises/01-eventsourcing/start/Profile.Api/Domain/Subscription.cs`.
Replace the content of the file with the following code:

```csharp
namespace Profile.Api.Domain;

public record Subscription(DateOnly StartDate, DateOnly? EndDate);
```

Finally, create a new file `exercises/01-eventsourcing/start/Profile.Api/Domain/Customer.cs`.
Add the following content to the end of the file:

```csharp
using Profile.Api.Shared;

namespace Profile.Api.Domain;

public class Customer: AggregateRoot
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
    
    protected override bool TryApplyDomainEvent(object domainEvent)
    {
        switch (domainEvent)
        {
            default:
                return false;
        }

        return true;
    }
}
```

The `Customer` class derives from the `AggregateRoot` class from the `Shared` namespace.
In the `Customer` class we've implemented the `TryApplyDomainEvent` with some place holder logic.
It works like this. We match the `domainEvent` parameter against a specific type. If there's a match,
we'll handle the event. If there's no match, the code will go through the `default:` case in the switch
and return `false` to the `Emit` method in the `AggregateRoot` class. In all other cases, we'll return `true` 
to the `Emit` method in the `AggregateRoot` class. 

Right now, the `TryApplyDomainEvent` method doesn't match any event since we don't have any domain events yet.
We'll change that next.

### Create the domain operations

We'll start by implementing the registration logic for a new customer. For this, we will add a public constructor
to the `Customer` class. A customer shouldn't exist if it weren't first registered. So we force the application to
construct a new customer as part of the registration process.

Create a new file `exercises/01-eventsourcing/start/Profile.Api/Domain/Events/CustomerRegistered.cs`.
Replace the content of the file with the following code:

```csharp
namespace Profile.Api.Domain.Events;

public record CustomerRegistered(
    Guid Id,
    string FirstName, 
    string LastName,
    Address InvoiceAddress,
    Address ShippingAddress,
    string EmailAddress);
```

Add the following code to the `Customer` class after the properties at the top of the class:

```csharp
public Customer(Guid id, string firstName, string lastName, Address invoiceAddress, Address shippingAddress, string emailAddress)
{
   Emit(new CustomerRegistered(id, firstName, lastName, invoiceAddress, shippingAddress, emailAddress));
}
```

The constructor takes the input and creates a new `CustomerRegistered` event. 
It then takes the event and sends it to the `Emit` method to execute the
event processing logic in the aggregate.

Next, we need to create a new method that updates the entity's state.
Add the following code to the end of the class file:

```csharp
private void Apply(CustomerRegistered customerRegistered)
{
    Id = customerRegistered.Id;
    FirstName = customerRegistered.FirstName;
    LastName = customerRegistered.LastName;
    InvoiceAddress = customerRegistered.InvoiceAddress;
    ShippingAddress = customerRegistered.ShippingAddress;
    EmailAddress = customerRegistered.EmailAddress;
    Subscription = new Subscription(DateOnly.FromDateTime(DateTime.UtcNow), null);
}
```

This method takes the event and updates the properties defined in the `Customer` class.

As a final step, we need to add a new case to the `switch` statement in the `TryApplyDomainEvent` method.
Modify the `TryApplyDomainEvent` method to match the following code:

```csharp
protected override bool TryApplyDomainEvent(object domainEvent)
{
    switch (domainEvent)
    {
        case CustomerRegistered customerRegistered:
            Apply(customerRegistered);
            break;
        default:
            return false;
    }

    return true;
}
```

Notice how we've placed the state updates in an event handler instead of the constructor.
Separating operations from event handlers will prove essential to our event-sourcing implementation later in the
exercise.

------------------------------------------------------------------------------------------------------------------------

**Important**  
Each method that modifies the state must be named `Apply`. We can use overloads of this method for each of the event
types. This is a requirement of Marten. When restoring an aggregate later, Marten will call the `Apply` method for each
event it reads from the event store.

------------------------------------------------------------------------------------------------------------------------

Now that you've implemented the first domain operation, you can repeat the process for the other operations that we
need. We'll need the following events in our code base:

* SubscriptionCanceled - Should be emitted by the `Unsubscribe` method.
* SubscriptionStarted - Should be emitted by the `Subscribe` method.

You can copy the event definitions from the directory `exercises/01-eventsourcing/solution/Profile.Api/Domain/Events/`.
After copying the event definitions, add the methods as described before. Make sure you add the event handlers 
for each domain event and update the `TryApplyDomainEvent` method to include your new event definitions.

When you've added all the operations, it's time to set up Marten for the first time so we can store the domain events.

## Configure the event store to persist the domain events

So far, we haven't talked about persisting domain events. That's for a good reason; our domain logic should be
independent of any infrastructure we use. Often, you'll find yourself writing domain logic first to learn about a
customer's business. After fully understanding a scenario, it would be best to spend time connecting to storage and a
communication interface.

Open `exercises/01-eventsourcing/start/Profile.Api/Program.cs` and search for a line starting with
`app.Services.AddMarten`. The logic you find there configures Marten in your application as a document store and looks
like this:

```csharp
builder.Services.AddMarten(marten =>
{
    marten.Connection(builder.Configuration.GetConnectionString("DefaultDatabase"));
    marten.AutoCreateSchemaObjects = AutoCreate.CreateOnly;
});
```

We need to provide Marten with a connection to a Postgres database. We must also tell it to populate the database when
it starts.

Now that we have Marten set up, we can connect the domain logic to the REST interface.

## Connect the domain logic to the REST interface

In the previous section, we've created our domain logic. Following along, you can test your logic using the 
provided unit-tests in the start or solution folder. You can run the tests by executing the following
command in your terminal from the directory `exercises/01-eventsourcing/start/`:

```shell
dotnet test
```

When you've verified that it works, we can move on to connecting the code to the REST API. We've added a controller to
the project for you, so you don't have to write all the code for the API yourself.

Open the file `exercises/01-eventsourcing/start/Profile.Api/Controllers/CustomersController.cs` and look at the
code. The file contains the following methods:

* RegisterCustomer
* CancelSubscription
* Resubscribe

Before we connect the domain logic to the controller, let's explore what we have.

In the constructor we've included a parameter of type `IDocumentSession`. This is a type that's provided by Marten.
It's an object that's created for each request that we execute against the API. The `IDocumentSession` represents
the active database connection.

You can use the `IDocumentSession` to insert, update, delete, and read documents in the database. The `IDocumentSession`
type also has an `Events` property that gives access to the event store mechanism provided by Marten. We will use the
event store mechanism to persist the events generated in the domain.

Let's start by implementing the RegisterCustomer operation.

### Registering a new customer

In the `CustomersController` class, find the `RegisterCustomer` method and add the following code to the method:

```csharp
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
```

This code performs the following steps:

1. First, we create a new customer instance from the provided input data.
2. Then, we read the list of generated domain events from the `Customer` instance and uses the `StartStream` method
   to start a new event stream for the customer.
3. Next, we save the changes to the database using `SaveChangesAsync`.
4. Finally, we return the Accepted HTTP status to the client of the API.

The `CreateStream` operation creates a new stream of events based on the domain events we generated in the `Customer`
aggregate. It doesn't store the event stream in the database yet. When we call `SaveChangesAsync`, a new transaction is
started, and the changes are saved.

It's nice that we can create a new event stream, but what about existing customers? Let's look at that next.

### Canceling a subscription

To cancel a subscription, we need to implement the `CancelSubscription` method in the API. We'll need to perform
the following steps: 

1. First, we need to retrieve the events for the customer from the database. 
2. Then, we need to restore the state of the customer.
3. Next, we need to execute the `Unsubscribe` method on the customer.
4. Finally, we need to append the newly generated domain events to the event stream.

Add the following code to the `CancelSubscription` method in the `CustomersController`:

```csharp
var customer = await _documentSession.Events.AggregateStreamAsync<Customer>(customerId);

if (customer == null)
{
    return NotFound();
}

customer.Unsubscribe();

_documentSession.Events.Append(customerId, customer.PendingDomainEvents);
await _documentSession.SaveChangesAsync();

return Accepted();
```

In this code, we perform the following steps:

1. First, we call the `AggregateStreamAsync<T>` method to load all events for the customer.
2. Next, we call the `Unsubscribe` method, which generates a new event.
3. Then, we call the `Append` method on the event store to add the new event to the event stream of the customer.
4. Finally, we use `SaveChangesAsync` to store the new events in the database.

We've now seen how to restore the state to an aggregate using the `AggregateStreamAsync` method. We've also learned
how to use the `Append` method on the event store to save new events to an existing event stream.

You can repeat the process that we used for `CancelSubscription` to implement the `Resubscribe` method.

## Summary and next steps

This first exercise taught us how to implement an event-sourced aggregate. We've also seen how to use Marten as
a pragmatic event store to persist events generated in our domain.

For the complete solution, check out the sources in [exercises/01-eventsource/solution](./solution/).

We'll extend the logic we created with projections for the next exercise. 

[Next exercise](../02-projections/README.md)
