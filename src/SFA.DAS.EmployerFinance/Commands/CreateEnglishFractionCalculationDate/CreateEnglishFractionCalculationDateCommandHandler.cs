using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Validation;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.NLog.Logger;
using System.Threading;

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

        public async Task<Unit> Handle(CreateEnglishFractionCalculationDateCommand message, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid())
            {
                throw new InvalidRequestException(validationResult.ValidationDictionary);
            }

            await _englishFractionRepository.SetLastUpdateDate(message.DateCalculated);

            _logger.Info($"English Fraction CalculationDate updated to {message.DateCalculated.ToString("dd MMM yyyy")}");

            return Unit.Value;
        }
    }
}
