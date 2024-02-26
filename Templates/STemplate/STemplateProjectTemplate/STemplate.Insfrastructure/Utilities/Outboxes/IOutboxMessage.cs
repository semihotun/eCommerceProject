﻿using STemplate.Insfrastructure.Utilities.ServiceBus;

namespace STemplate.Insfrastructure.Utilities.Outboxes;

public interface IOutboxMessage : IMessage
{
    public Guid EventId { get; set; }
    public State State { get; set; }
}
