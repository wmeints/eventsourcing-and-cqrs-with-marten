# Event-sourcing and CQRS with Marten

Welcome to the workshop "Event-sourcing and CQRS with Marten". In this workshop you'll learn how to apply event-sourcing
and separate write operations from read operations using CQRS (Command Query Responsibility Segregation).

This workshop has a couple of exercises that help you understand the most important concepts of event-sourcing and CQRS.

## About the case

This workshop is based on a virtual coffee company called TastyBeans. You can read more about the company below.

------------------------------------------------------------------------------------------------------------------------

Welcome to Tasty Beans, the subscription-based coffee beans shop that makes it easy to get your hands on the best beans
around!

With our recommendations feature, you can easily find the beans that suit your taste, and with our start-up guide, you
can get brewing in no time.

Our selection of beans is based on recommendations from coffee aficionados around the world. Whether you like your
coffee light and delicate or dark and bold, we have the perfect beans for you. Start your subscription today and enjoy
the best coffee beans delivered right to your door.

------------------------------------------------------------------------------------------------------------------------

We'll be building the profile service for the TastyBeans solution. This service has the following functional
requirements:

* New customers can register for a subscription
* Existing customers can cancel their subscription
* Customers who cancelled before, can resubscribe

## About the slides

This workshop comes with a presentation that you'll need to understand the concepts of event-sourcing and CQRS. 
You can find the slides [here](#).

## Excersises

We've made a couple of exercises that will get you started building an event-sourced system with Marten.
The first two exercises focus on building a domain and a couple of projections. The final exercise adds another library
called Marten, which helps you implement command and query handlers to implement CQRS.

1. Working with aggregates
2. Building read models
3. Applying CQRS principles

Each exercise can be done by itself. You can also choose to start from the first exercise and then move on to the next
and final exercise.

