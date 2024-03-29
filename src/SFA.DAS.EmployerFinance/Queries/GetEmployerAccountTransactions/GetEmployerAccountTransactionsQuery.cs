﻿namespace SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;

public class GetEmployerAccountTransactionsQuery : IRequest<GetEmployerAccountTransactionsResponse>
{
    public string HashedAccountId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
}