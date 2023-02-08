using MediatR;
using SFA.DAS.EmployerFinance.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using SFA.DAS.HashingService;
using System.Threading;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetEmployerAccount
{
    public class GetEmployerAccountHashedHandler : IRequestHandler<GetEmployerAccountHashedQuery, GetEmployerAccountResponse>
    {
        private readonly IEmployerAccountRepository _employerAccountRepository;
        private readonly IValidator<GetEmployerAccountHashedQuery> _validator;
        private readonly IHashingService _hashingService;

        public GetEmployerAccountHashedHandler(
            IEmployerAccountRepository employerAccountRepository,
            IValidator<GetEmployerAccountHashedQuery> validator,
            IHashingService hashingService)
        {
            _employerAccountRepository = employerAccountRepository;
            _validator = validator;
            _hashingService = hashingService;
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

            var accountId = _hashingService.DecodeValue(message.HashedAccountId);
            var employerAccount = await _employerAccountRepository.Get(accountId);

            return new GetEmployerAccountResponse
            {
                Account = employerAccount
            };
        }
    }
}