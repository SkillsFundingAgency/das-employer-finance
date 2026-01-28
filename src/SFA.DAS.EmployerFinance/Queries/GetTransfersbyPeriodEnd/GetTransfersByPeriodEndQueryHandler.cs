using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Queries.GetPeriodEnds;

namespace SFA.DAS.EmployerFinance.Queries.GetTransfersbyPeriodEnd;

public class GetTransfersByPeriodEndQueryHandler : IRequestHandler<GetTransfersByPeriodEndRequest, GetTransfersByPeriodEndResponse>
{
    private readonly IDasLevyRepository _dasLevyRepository;

    public GetTransfersByPeriodEndQueryHandler(IDasLevyRepository dasLevyRepository)
    {
        _dasLevyRepository = dasLevyRepository;
    }

    public async Task<GetTransfersByPeriodEndResponse> Handle(GetTransfersByPeriodEndRequest message, CancellationToken cancellationToken)
    {
        var response = new GetTransfersByPeriodEndResponse();

        var result = await _dasLevyRepository.GetTransfersByPeriodEnd(message.AccountId, message.PeriodEnd);
        response.AccountTransfers = result.ToList();
        return response;
    }
}
