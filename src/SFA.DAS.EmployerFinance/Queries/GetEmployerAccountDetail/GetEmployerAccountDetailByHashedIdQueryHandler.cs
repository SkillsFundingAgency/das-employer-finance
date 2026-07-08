using AutoMapper;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Validation;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.EmployerFinance.Queries.GetEmployerAccountDetail;

public class GetEmployerAccountDetailByHashedIdQueryHandler(
    IValidator<GetEmployerAccountDetailByHashedIdQuery> validator,
    IAccountApiClient accountApiClient,
    IMapper mapper)
    : IRequestHandler<GetEmployerAccountDetailByHashedIdQuery, GetEmployerAccountDetailByHashedIdResponse>
{
    public async Task<GetEmployerAccountDetailByHashedIdResponse> Handle(GetEmployerAccountDetailByHashedIdQuery message,CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(message);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var accountDetail = await accountApiClient.GetAccount(message.HashedAccountId);

        return new GetEmployerAccountDetailByHashedIdResponse 
        { 
            AccountDetail = mapper.Map<AccountDetailDto>(accountDetail) 
        };
    }
}