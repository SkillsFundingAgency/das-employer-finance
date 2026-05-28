namespace SFA.DAS.EmployerFinance.Queries.GetLastEnglishFractionCalculationDate;

public class GetLastEnglishFractionCalculationDateQuery : IRequest<GetLastEnglishFractionCalculationDateResponse>
{
    public string EmpRef { get; set; }
}
