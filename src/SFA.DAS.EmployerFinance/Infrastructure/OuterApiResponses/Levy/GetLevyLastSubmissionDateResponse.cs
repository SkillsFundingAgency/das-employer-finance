namespace SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;

public sealed record GetLevyLastSubmissionDateResponse
{
    public DateTime LatestLevyDeclarationInDate { get; init; }
}