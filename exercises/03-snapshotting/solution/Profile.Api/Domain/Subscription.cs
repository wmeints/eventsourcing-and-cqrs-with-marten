namespace Profile.Api.Domain;

public record Subscription(DateOnly StartDate, DateOnly? EndDate);