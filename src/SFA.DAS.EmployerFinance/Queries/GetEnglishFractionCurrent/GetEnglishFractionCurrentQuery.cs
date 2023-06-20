namespace SFA.DAS.EmployerFinance.Queries.GetEnglishFractionCurrent;

public class GetEnglishFractionCurrentQuery : IRequest<GetEnglishFractionCurrentResponse>
{
    public string HashedAccountId { get; set; }
    public string[] EmpRefs { get; set; }
}