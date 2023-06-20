using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetEmployerAccount;

public class GetEmployerAccountHashedHandler : IRequestHandler<GetEmployerAccountHashedQuery, GetEmployerAccountResponse>
{
    private readonly IEmployerAccountRepository _employerAccountRepository;
    private readonly IValidator<GetEmployerAccountHashedQuery> _validator;
    private readonly IEncodingService _encodingService;

    public GetEmployerAccountHashedHandler(
        IEmployerAccountRepository employerAccountRepository,
        IValidator<GetEmployerAccountHashedQuery> validator,
        IEncodingService encodingService)
    {
        _employerAccountRepository = employerAccountRepository;
        _validator = validator;
        _encodingService = encodingService;
    }

    public async Task<GetEmployerAccountResponse> Handle(GetEmployerAccountHashedQuery message,CancellationToken cancellationToken)
    {
        var result = await _validator.ValidateAsync(message);

        if (!result.IsValid())
        {
            throw new ValidationException(result.ConvertToDataAnnotationsValidationResult(),null,null);
        }

        if (result.IsUnauthorized)
        {
            throw new UnauthorizedAccessException();
        }

        var accountId = _encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);
        var employerAccount = await _employerAccountRepository.Get(accountId);

        return new GetEmployerAccountResponse
        {
            Account = employerAccount
        };
    }
}