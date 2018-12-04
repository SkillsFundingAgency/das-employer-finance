﻿using System;
using SFA.DAS.NServiceBus;

namespace SFA.DAS.EmployerAccounts.Messages.Events
{
    public class UserRolesRemovedEvent : Event
    {
        public long AccountId { get; }
        public Guid UserRef { get; }
        public UserRolesRemovedEvent(long accountId, Guid userRef, DateTime created)
        {
            AccountId = accountId;
            UserRef = userRef;
            Created = created;
        }
    }
}
