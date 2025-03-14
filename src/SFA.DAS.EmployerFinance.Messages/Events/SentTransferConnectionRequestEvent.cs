﻿using System;

namespace SFA.DAS.EmployerFinance.Messages.Events
{
    public class SentTransferConnectionRequestEvent
    {
        public long ReceiverAccountId { get; set; }
        public string ReceiverAccountName { get; set; }
        public long SenderAccountId { get; set; }
        public string SenderAccountName { get; set; }
        public long SentByUserId { get; set; }
        public string SentByUserName { get; set; }
        public Guid SentByUserRef { get; set; }
        public int TransferConnectionRequestId { get; set; }
        public DateTime Created { get; set; }
    }
}
