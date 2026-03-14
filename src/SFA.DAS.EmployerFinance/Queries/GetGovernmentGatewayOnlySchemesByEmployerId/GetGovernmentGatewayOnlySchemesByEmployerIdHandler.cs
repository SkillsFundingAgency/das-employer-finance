using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetGovernmentGatewayOnlySchemesByEmployerId;

public class GetGovernmentGatewayOnlySchemesByEmployerIdHandler : IRequestHandler<GetGovernmentGatewayOnlySchemesByEmployerIdQuery, GetGovernmentGatewayOnlySchemesByEmployerIdResponse>
{
    private readonly IValidator<GetGovernmentGatewayOnlySchemesByEmployerIdQuery> _validator;
    private readonly IPayeRepository _payeRepository;
    private readonly IEmployerAccountRepository _employerAccountRepository;

    public GetGovernmentGatewayOnlySchemesByEmployerIdHandler(
        IValidator<GetGovernmentGatewayOnlySchemesByEmployerIdQuery> validator,
        IPayeRepository payeRepository,
        IEmployerAccountRepository employerAccountRepository)
    {
        _validator = validator;
        _payeRepository = payeRepository;
        _employerAccountRepository = employerAccountRepository;
    }

    public async Task<GetGovernmentGatewayOnlySchemesByEmployerIdResponse> Handle(GetGovernmentGatewayOnlySchemesByEmployerIdQuery request, CancellationToken cancellationToken)
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

        var schemes = await _payeRepository.GetGovernmentGatewayOnlySchemesByEmployerId(request.AccountId);
        var schemeList = schemes?.SchemesList ?? new List<Models.Paye.Paye>();

        return new GetGovernmentGatewayOnlySchemesByEmployerIdResponse
        {
            Schemes = schemeList
        };
    }
}
