using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Validation;
using SFA.DAS.EmployerFinance.Data;
using System.Threading;

namespace SFA.DAS.EmployerFinance.Commands.CreateNewPeriodEnd
{
    public class CreateNewPeriodEndCommandHandler : IRequestHandler<CreateNewPeriodEndCommand,Unit>
    {
        private readonly IValidator<CreateNewPeriodEndCommand> _validator;
        private readonly IDasLevyRepository _dasLevyRepository;

        public CreateNewPeriodEndCommandHandler(IValidator<CreateNewPeriodEndCommand> validator, IDasLevyRepository dasLevyRepository)
        {
            _validator = validator;
            _dasLevyRepository = dasLevyRepository;
        }

        public async Task<Unit> Handle(CreateNewPeriodEndCommand message, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid())
            {
                throw new InvalidRequestException(validationResult.ValidationDictionary);
            }

            await _dasLevyRepository.CreateNewPeriodEnd(message.NewPeriodEnd);
            
            return Unit.Value;
        }
    }
}