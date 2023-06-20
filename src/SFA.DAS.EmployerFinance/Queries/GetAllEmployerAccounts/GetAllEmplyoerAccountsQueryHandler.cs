using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetAllEmployerAccounts;

public class GetAllEmplyoerAccountsQueryHandler : IRequestHandler<GetAllEmployerAccountsRequest, GetAllEmployerAccountsResponse>
{
    private readonly IEmployerAccountRepository _employerAccountRepository;

    public GetAllEmplyoerAccountsQueryHandler(IEmployerAccountRepository employerAccountRepository)
    {
        _employerAccountRepository = employerAccountRepository;
    }

    public async Task<GetAllEmployerAccountsResponse> Handle(GetAllEmployerAccountsRequest message,CancellationToken cancellationToken)
    {
        var result = await _employerAccountRepository.GetAll();

        return new GetAllEmployerAccountsResponse {Accounts = result };
    }
}