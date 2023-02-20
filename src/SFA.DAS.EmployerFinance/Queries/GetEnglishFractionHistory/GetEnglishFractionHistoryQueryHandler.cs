using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.GetEnglishFrationHistory
{
    public class GetEnglishFractionHistoryQueryHandler : IRequestHandler<GetEnglishFractionHistoryQuery, GetEnglishFractionHistoryResposne>
    {
        private readonly IValidator<GetEnglishFractionHistoryQuery> _validator;
        private readonly IDasLevyRepository _dasLevyRepository;
        private readonly IEncodingService _encodingService;

        public GetEnglishFractionHistoryQueryHandler(IValidator<GetEnglishFractionHistoryQuery> validator, IDasLevyRepository dasLevyRepository, IEncodingService encodingService)
        {
            _validator = validator;
            _dasLevyRepository = dasLevyRepository;
            _encodingService = encodingService;
        }

        public async Task<GetEnglishFractionHistoryResposne> Handle(GetEnglishFractionHistoryQuery message,CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var accountId =     _encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);
            var fractionDetail = await _dasLevyRepository.GetEnglishFractionHistory(accountId, message.EmpRef);

            return new GetEnglishFractionHistoryResposne {FractionDetail = fractionDetail};
        }
    }
}
