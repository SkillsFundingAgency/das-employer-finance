using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using System.Threading;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.EmployerFinance.Data.Contracts;

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

        public async Task<Unit> Handle(CreateNewPeriodEndCommand request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            await _dasLevyRepository.CreateNewPeriodEnd(request.NewPeriodEnd);
            
            return Unit.Value;
        }
    }
}