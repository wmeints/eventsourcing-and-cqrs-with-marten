# Exercise 3 - Working with snapshots

In the previous exercise we spend some time to build a projection for querying customer information in the profile
service. In this exercise we're going to introduce another important aspect to optimize an event-sourced system for
speed. We're going to add support for snapshots.

Let's get started by taking a snapshot first. For this example, we're going to take a snapshot every time the customer
cancels a subscription. In any regular service you would not take a snapshot that many times. Because snapshots
ultimately will slow down your service as well. Just less fast, because you're taking far fewer snapshots.

## Step 1 - Expand aggregate with version information

Before we can make a snapshot, we need to implement logic to record the current version of the aggregate. We need this
information to restore the snapshot and then replay the events that happened after the snapshot was taken. 

We've added a new property `public long Version { get; protected set; }` to the file
`exercises/03-snapshotting/start/Profile.Api/Shared/AggregateRoot.cs`. 

Add the following line to each of the `Apply` methods in the file
`exercises/03-snapshotting/start/Profile.Api/Domain/Customer.cs`:

```csharp
Version++;
```

Whenever an event is applied to the aggregate, we're increasing the version number. This information helps us later when
we record a snapshot. It's also a useful property to use when you need [optimistic concurrency
control][CONCURRENCY_CONTROL].

Now that we've versioned the aggregate, let's store a snapshot.

## Step 2 - Record the snapshot

We need to add logic to save a snapshot of the `Customer` class. We're going to add this logic to the controller that's
used to cancel the subscription of a customer.

Open `exercises/03-snapshotting/start/Profile.Api/Controllers/CustomersController.cs` and add the following code to the
after the line that calls `_documentSession.Events.Append` in the method `CancelSubscription`:

```csharp
_documentSession.Store(customer); // This creates the snapshot for us.
```

This stores the customer as a regular document. Please note, if we had a previous snapshot of the customer we'll
overwrite it with the new one. This is because we're calling `Save`. Currently, Marten doesn't support taking multiple
snapshots. It's not a huge problem though and we expect that it will get added in a future version.

With the snapshot recorded, we can use the snapshot when trying to resubscribe a customer.

## Step 3 - Restore from a snapshot

Open the file `exercises/03-snapshotting/start/Profile.Api/Controllers/CustomersController.cs` and replace the content of
the `StartSubscription` method with the following content:

```csharp
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
```

In the new logic, we try to restore a snapshot. If we find a snapshot, we replay the events, starting at the snapshot 
version. When we can't find a snapshot, we'll instead replay everything from version 0 onward. If, in the end, we can't 
restore the customer state, we abandon the rest of the operation.

The start subscription now supports restoring from snapshots. You can use the first line that restore the customer
state from a snapshot or the stream in the `CancelSubscription` method to complete this exercise.

## Summary

In this exercise we've learned how to work with snapshots. For the purpose of the exercise, we implemented the snapshot
restore code directly in the controller. You can improve upon this by using a `Repository` that handles the snapshot 
logic for you. 

After completing all the exercises you should understand enough about event-sourcing to implement the `Repository` 
pattern with optimistic concurrency control without a lot of problems.

We hope you enjoyed this workshop. Please star this repository if you've followed this workshop :smile:

[CONCURRENCY_CONTROL]: https://martendb.io/scenarios/aggregates-events-repositories.html#scenario
