using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetPeriodEnds;

public class GetPeriodEndByPeriodEndIdQueryHandler : IRequestHandler<GetPeriodEndByPeriodEndIdRequest, GetPeriodEndByPeriodEndIdResponse>
{
    private readonly IDasLevyRepository _dasLevyRepository;

    public GetPeriodEndByPeriodEndIdQueryHandler(IDasLevyRepository dasLevyRepository)
    {
        _dasLevyRepository = dasLevyRepository;
    }

    public async Task<GetPeriodEndByPeriodEndIdResponse> Handle(GetPeriodEndByPeriodEndIdRequest message,CancellationToken cancellationToken)
    {
        var response = new GetPeriodEndByPeriodEndIdResponse();

        response.PeriodEnd = await _dasLevyRepository.GetPeriodEndById(message.PeriodEndId);

        return response;
    }
}