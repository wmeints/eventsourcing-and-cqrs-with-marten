# Exercise 3 - Applying CQRS principles

In the previous exercise we spend some time to build a projection for querying customer information in the profile
service. In this exercise we're going to introduce another important aspect to optimize an event-sourced system for
speed. We're going to add support for snapshots.

------------------------------------------------------------------------------------------------------------------------

**Why would you use a snapshot?**  
When the number of events for a single event stream grows, you'll notice that your application becomes slower when
trying to execute operations against an event-sourced aggregate. 

The slowdown in your service is caused by the fact that the service needs to replay each event for an aggregate before
it can append another event to it. Otherwise you would append an event without knowing about the current state.

You can of course not replay events and just call `Append` on the `IDocumentSession.Events` property. But that comes
with a great risk since you're not validating if the new event is valid. There are cases where it's not a problem. But
for most cases you want to use a slightly different approach.

A better approach to make your service faster is to use a snapshot. A snapshot is essentially a copy of the state
produced by events that happened before. Before replaying any events, you load the snapshot, and then replay the events
that happened after the snapshot was taken.

------------------------------------------------------------------------------------------------------------------------

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
_documentSession.Save(customer); // This creates the snapshot for us.
```

This stores the customer as a regular document. Please note, if we had a previous snapshot of the customer we'll
overwrite it with the new one. This is because we're calling `Save`. Currently, Marten doesn't support taking multiple
snapshots. It's not a huge problem though and we expect that it will get added in a future version.

With the snapshot recorded, we can use the snapshot when trying to resubscribe a customer.

## Step 3 - Restore from a snapshot

Open the file `exercises/03-snapshotting/start/Profile.Api/Controllers/CustomersController.cs` and replace the content of
the `StartSubscription` method with the following content:

```csharp

```

[CONCURRENCY_CONTROL]: https://martendb.io/scenarios/aggregates-events-repositories.html#scenario