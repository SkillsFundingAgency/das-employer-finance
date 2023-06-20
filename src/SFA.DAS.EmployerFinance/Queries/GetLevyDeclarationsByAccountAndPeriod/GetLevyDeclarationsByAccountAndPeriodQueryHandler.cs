using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationsByAccountAndPeriod;

public class GetLevyDeclarationsByAccountAndPeriodQueryHandler : IRequestHandler<GetLevyDeclarationsByAccountAndPeriodRequest, GetLevyDeclarationsByAccountAndPeriodResponse>
{
    private readonly IDasLevyRepository _repository;
    private readonly IEncodingService _encodingService;

    public GetLevyDeclarationsByAccountAndPeriodQueryHandler(IDasLevyRepository repository, IEncodingService encodingService)
    {
        _repository = repository;
        _encodingService = encodingService;
    }

    public async Task<GetLevyDeclarationsByAccountAndPeriodResponse> Handle(GetLevyDeclarationsByAccountAndPeriodRequest message,CancellationToken cancellationToken)
    {
        var accountId = _encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);
        var declarations = await _repository.GetAccountLevyDeclarations(accountId, message.PayrollYear, message.PayrollMonth);
        
        return new GetLevyDeclarationsByAccountAndPeriodResponse { Declarations = declarations };
    }
}