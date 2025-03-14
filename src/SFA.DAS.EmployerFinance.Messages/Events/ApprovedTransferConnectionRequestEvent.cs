﻿using System;

namespace SFA.DAS.EmployerFinance.Messages.Events
{
    public class ApprovedTransferConnectionRequestEvent
    {
        public long ApprovedByUserId { get; set; }
        public string ApprovedByUserName { get; set; }
        public Guid ApprovedByUserRef { get; set; }
        public long ReceiverAccountId { get; set; }
        public string ReceiverAccountName { get; set; }
        public long SenderAccountId { get; set; }
        public string SenderAccountName { get; set; }
        public int TransferConnectionRequestId { get; set; }
        public DateTime Created { get; set; }
    }
}
