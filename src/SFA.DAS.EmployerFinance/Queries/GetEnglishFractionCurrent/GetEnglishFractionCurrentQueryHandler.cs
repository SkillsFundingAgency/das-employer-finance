using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.HashingService;

namespace SFA.DAS.EmployerFinance.Queries.GetEnglishFractionCurrent
{
    public class GetEnglishFractionCurrentQueryHandler : IRequestHandler<GetEnglishFractionCurrentQuery, GetEnglishFractionCurrentResponse>
    {
        private readonly IValidator<GetEnglishFractionCurrentQuery> _validator;
        private readonly IDasLevyRepository _dasLevyRepository;
        private readonly IHashingService _hashingService;

        public GetEnglishFractionCurrentQueryHandler(IValidator<GetEnglishFractionCurrentQuery> validator, IDasLevyRepository dasLevyRepository, IHashingService hashingService)
        {
            _validator = validator;
            _dasLevyRepository = dasLevyRepository;
            _hashingService = hashingService;
        }

        public async Task<GetEnglishFractionCurrentResponse> Handle(GetEnglishFractionCurrentQuery message,CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var accountId = _hashingService.DecodeValue(message.HashedAccountId);
            var fractions = await _dasLevyRepository.GetEnglishFractionCurrent(accountId, message.EmpRefs);

            return new GetEnglishFractionCurrentResponse { Fractions = fractions};
        }
    }
}
