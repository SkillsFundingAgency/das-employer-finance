using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.NLog.Logger;
using System.Threading;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Commands.CreateEnglishFractionCalculationDate
{
    public class CreateEnglishFractionCalculationDateCommandHandler : IRequestHandler<CreateEnglishFractionCalculationDateCommand,Unit>
    {
        private readonly IValidator<CreateEnglishFractionCalculationDateCommand> _validator;
        private readonly IEnglishFractionRepository _englishFractionRepository;
        private readonly ILog _logger;

        public CreateEnglishFractionCalculationDateCommandHandler(IValidator<CreateEnglishFractionCalculationDateCommand> validator, IEnglishFractionRepository englishFractionRepository, ILog logger)
        {
            _validator = validator;
            _englishFractionRepository = englishFractionRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(CreateEnglishFractionCalculationDateCommand request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            await _englishFractionRepository.SetLastUpdateDate(request.DateCalculated);

            _logger.Info($"English Fraction CalculationDate updated to {request.DateCalculated.ToString("dd MMM yyyy")}");

            return Unit.Value;
        }
    }
}
