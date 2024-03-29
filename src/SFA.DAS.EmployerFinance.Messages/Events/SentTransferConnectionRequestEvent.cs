﻿using System;
using SFA.DAS.NServiceBus;

namespace SFA.DAS.EmployerFinance.Messages.Events
{
    public class SentTransferConnectionRequestEvent : Event
    {
        public long ReceiverAccountId { get; set; }
        public string ReceiverAccountName { get; set; }
        public long SenderAccountId { get; set; }
        public string SenderAccountName { get; set; }
        public long SentByUserId { get; set; }
        public string SentByUserName { get; set; }
        public Guid SentByUserRef { get; set; }
        public int TransferConnectionRequestId { get; set; }
    }
}
