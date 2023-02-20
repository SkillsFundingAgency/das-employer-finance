using System.ComponentModel.DataAnnotations;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountBalances
{
    public class GetAccountBalancesQueryHandler : IRequestHandler<GetAccountBalancesRequest, GetAccountBalancesResponse>
    {
        private readonly IDasLevyRepository _dasLevyRepository;
        private readonly IValidator<GetAccountBalancesRequest> _validator;

        public GetAccountBalancesQueryHandler(IDasLevyRepository dasLevyRepository, IValidator<GetAccountBalancesRequest> validator)
        {
            _dasLevyRepository = dasLevyRepository;
            _validator = validator;
        }

        public async Task<GetAccountBalancesResponse> Handle(GetAccountBalancesRequest message,CancellationToken cancellationToken)
        {

            var validationResult = _validator.Validate(message);
            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var result = await _dasLevyRepository.GetAccountBalances(message.AccountIds);

            return new GetAccountBalancesResponse { Accounts = result };
        }
    }
}
