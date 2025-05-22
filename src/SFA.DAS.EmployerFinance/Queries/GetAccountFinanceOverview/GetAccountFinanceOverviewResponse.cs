﻿namespace SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;

public class GetAccountFinanceOverviewResponse
{
    public long AccountId { get; set; }
    public decimal CurrentFunds { get; set; }
    public decimal TotalSpendForLastYear { get; set; }
    public decimal FundsIn { get; set; }
    public decimal FundsOut { get; set; }
}