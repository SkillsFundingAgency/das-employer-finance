namespace SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;

public sealed record GetLevyProjectionsStatusByAccountIdResponse
{
    public DateTime LastSuccessfulImportDate { get; init; }
}
