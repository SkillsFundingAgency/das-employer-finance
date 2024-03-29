﻿using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Queries.FindAccountProviderPayments;

public class FindAccountProviderPaymentsResponse
{
    public string ProviderName { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime DateCreated { get; set; }
    public List<PaymentTransactionLine> Transactions { get; set; }
    public decimal Total { get; set; }
}