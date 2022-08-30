namespace Profile.Api.Domain.Events;

public record SubscriptionCanceled(Guid Id, DateOnly EndDate);