using System.ComponentModel.DataAnnotations;
using AutoMapper;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetEmployerAccountDetail;

public class GetEmployerAccountDetailByHashedIdQueryHandler : IRequestHandler<GetEmployerAccountDetailByHashedIdQuery, GetEmployerAccountDetailByHashedIdResponse>
{
    private readonly IValidator<GetEmployerAccountDetailByHashedIdQuery> _validator;
    private readonly IAccountApiClient _accountApiClient;
    private readonly IMapper _mapper;

    public GetEmployerAccountDetailByHashedIdQueryHandler(IValidator<GetEmployerAccountDetailByHashedIdQuery> validator, IAccountApiClient accountApiClient, IMapper mapper)
    {
        _validator = validator;
        _accountApiClient = accountApiClient;
        _mapper = mapper;
    }

    public async Task<GetEmployerAccountDetailByHashedIdResponse> Handle(GetEmployerAccountDetailByHashedIdQuery message,CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(message);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var accountDetail = await _accountApiClient.GetAccount(message.HashedAccountId);

        return new GetEmployerAccountDetailByHashedIdResponse 
        { 
            AccountDetail = _mapper.Map<AccountDetailDto>(accountDetail) 
        };
    }
}