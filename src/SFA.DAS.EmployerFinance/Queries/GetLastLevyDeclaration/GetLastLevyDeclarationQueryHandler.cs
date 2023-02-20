using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using System.Threading;
using Microsoft.ServiceBus;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclaration
{
    public class GetLastLevyDeclarationQueryHandler : IRequestHandler<GetLastLevyDeclarationQuery, GetLastLevyDeclarationResponse>
    {
        private readonly IValidator<GetLastLevyDeclarationQuery> _validator;
        private readonly IDasLevyRepository _dasLevyRepository;
        
        public GetLastLevyDeclarationQueryHandler(IValidator<GetLastLevyDeclarationQuery> validator, IDasLevyRepository dasLevyRepository)
        {
            _validator = validator;
            _dasLevyRepository = dasLevyRepository;
        }

        public async Task<GetLastLevyDeclarationResponse> Handle(GetLastLevyDeclarationQuery message,CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null,null);
            }

            var result = await _dasLevyRepository.GetLastSubmissionForScheme(message.EmpRef);
            
            return new GetLastLevyDeclarationResponse {Transaction = result };
        }
    }
}