namespace SFA.DAS.EmployerFinance.Queries.GetEnglishFractionHistory;

public class GetEnglishFractionHistoryQuery : IRequest<GetEnglishFractionHistoryResposne>
{
    public string HashedAccountId { get; set; }
    public string EmpRef { get; set; }
}