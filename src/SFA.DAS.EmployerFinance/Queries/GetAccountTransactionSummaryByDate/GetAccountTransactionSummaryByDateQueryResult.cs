using SFA.DAS.EmployerFinance.Models.Transaction;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountTransactionSummaryByDate;

public sealed record GetAccountTransactionSummaryByDateQueryResult(TransactionLine[] Data);