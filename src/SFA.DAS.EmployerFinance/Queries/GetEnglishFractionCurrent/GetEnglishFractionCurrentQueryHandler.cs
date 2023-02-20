using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.GetEnglishFractionCurrent
{
    public class GetEnglishFractionCurrentQueryHandler : IRequestHandler<GetEnglishFractionCurrentQuery, GetEnglishFractionCurrentResponse>
    {
        private readonly IValidator<GetEnglishFractionCurrentQuery> _validator;
        private readonly IDasLevyRepository _dasLevyRepository;
        private readonly IEncodingService _encodingService;

        public GetEnglishFractionCurrentQueryHandler(IValidator<GetEnglishFractionCurrentQuery> validator, IDasLevyRepository dasLevyRepository, IEncodingService encodingService)
        {
            _validator = validator;
            _dasLevyRepository = dasLevyRepository;
            _encodingService = encodingService;
        }

        public async Task<GetEnglishFractionCurrentResponse> Handle(GetEnglishFractionCurrentQuery message,CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var accountId = _encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);
            var fractions = await _dasLevyRepository.GetEnglishFractionCurrent(accountId, message.EmpRefs);

            return new GetEnglishFractionCurrentResponse { Fractions = fractions};
        }
    }
}
