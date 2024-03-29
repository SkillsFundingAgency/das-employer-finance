﻿namespace SFA.DAS.EmployerFinance.Queries.FindEmployerAccountLevyDeclarationTransactions;

public class FindEmployerAccountLevyDeclarationTransactionsQuery : IRequest<FindEmployerAccountLevyDeclarationTransactionsResponse>
{
    public string HashedAccountId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}