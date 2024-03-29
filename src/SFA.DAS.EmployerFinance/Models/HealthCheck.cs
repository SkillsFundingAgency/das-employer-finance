﻿using SFA.DAS.EmployerFinance.Messages.Events;

namespace SFA.DAS.EmployerFinance.Models;

public class HealthCheck : Entity
{
    public virtual int Id { get; protected set; }
    public virtual Guid UserRef { get; protected set; }
    public virtual DateTime SentRequest { get; protected set; }
    public virtual DateTime? ReceivedResponse { get; protected set; }
    public virtual DateTime PublishedEvent { get; protected set; }
    public virtual DateTime? ReceivedEvent { get; protected set; }

    public HealthCheck(Guid userRef)
    {
        UserRef = userRef;
    }

    protected HealthCheck()
    {
    }

    public async Task Run(Func<Task> request)
    {
        await SendRequest(request);
        PublishEvent();
    }

    public void ReceiveEvent(HealthCheckEvent message)
    {
        ReceivedEvent = DateTime.UtcNow;
    }

    private async Task SendRequest(Func<Task> run)
    {
        SentRequest = DateTime.UtcNow;

        try
        {
            await run();
            ReceivedResponse = DateTime.UtcNow;
        }
        catch
        {
            // If exception caught, received response will remain default (?)
        }
    }

    private void PublishEvent()
    {
        PublishedEvent = DateTime.UtcNow;

        Publish(() => new HealthCheckEvent
        {
            Id = Id,
            Created = PublishedEvent
        });
    }
}