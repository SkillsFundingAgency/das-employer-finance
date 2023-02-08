using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.Hmrc;
using System.Threading;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.UpdatePayeInformation
{
    public class UpdatePayeInformationCommandHandler : IRequestHandler<UpdatePayeInformationCommand, Unit>
    {
        private readonly IValidator<UpdatePayeInformationCommand> _validator;
        private readonly IPayeRepository _payeRepository;
        private readonly IHmrcService _hmrcService;

        public UpdatePayeInformationCommandHandler(IValidator<UpdatePayeInformationCommand> validator, IPayeRepository payeRepository, IHmrcService hmrcService)
        {
            _validator = validator;
            _payeRepository = payeRepository;
            _hmrcService = hmrcService;
        }

        public async Task<Unit> Handle(UpdatePayeInformationCommand request,CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var scheme = await _payeRepository.GetPayeSchemeByRef(request.PayeRef);

            if (!string.IsNullOrEmpty(scheme?.Name))
            {
               //return;
            }

            var result = await _hmrcService.GetEmprefInformation(scheme?.EmpRef);

            if (string.IsNullOrEmpty(result?.Employer?.Name?.EmprefAssociatedName))
            {
                //return;
            }

            await _payeRepository.UpdatePayeSchemeName(request.PayeRef, result.Employer.Name.EmprefAssociatedName);

            return Unit.Value;
        }
    }
}