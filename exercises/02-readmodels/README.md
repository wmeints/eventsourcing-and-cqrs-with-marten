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

## Step 1 - Add a new projection to the service

## Step 2 - Register the projection with Marten

## Step 3 - Use the read model from the service

## Summary

