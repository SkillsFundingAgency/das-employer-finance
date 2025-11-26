using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetAccounts;

public class GetAccountsQueryHandler : IRequestHandler<GetAccountsRequest, GetAccountsResponse>
{
    private readonly IDasLevyRepository _dasLevyRepository;

    public GetAccountsQueryHandler(IDasLevyRepository dasLevyRepository)
    {
        _dasLevyRepository = dasLevyRepository;
    }

    public async Task<GetAccountsResponse> Handle(GetAccountsRequest message, CancellationToken cancellationToken)
    {
        var response = new GetAccountsResponse();

        return await _dasLevyRepository.GetAccounts(message.PageSize, message.PageNumber);
    }
}
