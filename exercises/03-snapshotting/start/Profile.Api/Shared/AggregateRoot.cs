﻿namespace Profile.Api.Shared;

public abstract class AggregateRoot
{
    private readonly List<object> _pendingDomainEvents = new();

    public Guid Id { get; protected set; }
    public long Version { get; protected set; }
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
