﻿using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerFinance.Api.Types
{
    public class Transaction
    {
        public string Description { get; set; }
        public TransactionItemType TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime DateCreated { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public List<Transaction> SubTransactions { get; set; }
        public string ResourceUri { get; set; }
    }
}
