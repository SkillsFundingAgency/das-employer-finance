using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Validation;
using SFA.DAS.EmployerFinance.Data;
using System.Threading;

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
                throw new InvalidRequestException(validationResult.ValidationDictionary);
            }

            var result = await _dasLevyRepository.GetLastSubmissionForScheme(message.EmpRef);
            
            return new GetLastLevyDeclarationResponse {Transaction = result };
        }
    }
}