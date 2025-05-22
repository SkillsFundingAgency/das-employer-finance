using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Transfers;

namespace SFA.DAS.EmployerFinance.Services.Contracts;

public interface ITransfersService
{
    Task<GetCountsResponse> GetCounts(long accountId);
}