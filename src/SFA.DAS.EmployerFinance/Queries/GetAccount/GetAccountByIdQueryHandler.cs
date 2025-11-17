using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetAccount;

public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdRequest, GetAccountByIdResponse>
{
    private readonly IDasLevyRepository _dasLevyRepository;

    public GetAccountByIdQueryHandler(IDasLevyRepository dasLevyRepository)
    {
        _dasLevyRepository = dasLevyRepository;
    }

    public async Task<GetAccountByIdResponse> Handle(GetAccountByIdRequest message, CancellationToken cancellationToken)
    {
        var response = new GetAccountByIdResponse();

        response.Account = await _dasLevyRepository.GetAccountById(message.AccountId);

        return response;
    }
}
