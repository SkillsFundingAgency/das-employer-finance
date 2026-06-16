using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetPayeSchemesByEmployerId;

public class GetPayeSchemesByEmployerIdHandler : IRequestHandler<GetPayeSchemesByEmployerIdQuery, GetPayeSchemesByEmployerIdResponse>
{
    private const string GovernmentGatewaySource = "government-gateway";

    private readonly IValidator<GetPayeSchemesByEmployerIdQuery> _validator;
    private readonly IPayeRepository _payeRepository;
    private readonly IEmployerAccountRepository _employerAccountRepository;

    public GetPayeSchemesByEmployerIdHandler(
        IValidator<GetPayeSchemesByEmployerIdQuery> validator,
        IPayeRepository payeRepository,
        IEmployerAccountRepository employerAccountRepository)
    {
        _validator = validator;
        _payeRepository = payeRepository;
        _employerAccountRepository = employerAccountRepository;
    }

    public async Task<GetPayeSchemesByEmployerIdResponse> Handle(GetPayeSchemesByEmployerIdQuery request, CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(request);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var account = await _employerAccountRepository.Get(request.AccountId);
        if (account == null)
        {
            return null;
        }

        var schemes = string.Equals(request.Source, GovernmentGatewaySource, StringComparison.OrdinalIgnoreCase)
            ? await _payeRepository.GetGovernmentGatewayOnlySchemesByEmployerId(request.AccountId)
            : await _payeRepository.GetSchemesByEmployerId(request.AccountId);

        return new GetPayeSchemesByEmployerIdResponse
        {
            Schemes = schemes?.SchemesList ?? new List<Models.Paye.Paye>()
        };
    }
}
