﻿namespace SFA.DAS.EmployerFinance.Queries.FindAccountProviderPayments;

public class FindAccountProviderPaymentsQuery : IRequest<FindAccountProviderPaymentsResponse>
{
    public string HashedAccountId { get; set; }
    public long UkPrn { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}