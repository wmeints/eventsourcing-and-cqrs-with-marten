namespace Profile.Api.Domain.Events;

public record SubscriptionStarted(Guid Id, DateOnly StartDate);