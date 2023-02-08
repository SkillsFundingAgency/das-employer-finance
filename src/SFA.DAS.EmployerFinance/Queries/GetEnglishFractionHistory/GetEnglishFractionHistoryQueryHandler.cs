using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.HashingService;

namespace SFA.DAS.EmployerFinance.Queries.GetEnglishFrationHistory
{
    public class GetEnglishFractionHistoryQueryHandler : IRequestHandler<GetEnglishFractionHistoryQuery, GetEnglishFractionHistoryResposne>
    {
        private readonly IValidator<GetEnglishFractionHistoryQuery> _validator;
        private readonly IDasLevyRepository _dasLevyRepository;
        private readonly IHashingService _hashingService;

        public GetEnglishFractionHistoryQueryHandler(IValidator<GetEnglishFractionHistoryQuery> validator, IDasLevyRepository dasLevyRepository, IHashingService hashingService)
        {
            _validator = validator;
            _dasLevyRepository = dasLevyRepository;
            _hashingService = hashingService;
        }

        public async Task<GetEnglishFractionHistoryResposne> Handle(GetEnglishFractionHistoryQuery message,CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var accountId = _hashingService.DecodeValue(message.HashedAccountId);
            var fractionDetail = await _dasLevyRepository.GetEnglishFractionHistory(accountId, message.EmpRef);

            return new GetEnglishFractionHistoryResposne {FractionDetail = fractionDetail};
        }
    }
}
