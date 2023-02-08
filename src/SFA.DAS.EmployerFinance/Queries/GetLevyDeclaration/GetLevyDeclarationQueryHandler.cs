using System.ComponentModel.DataAnnotations;
using MediatR;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.HashingService;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetLevyDeclaration
{
    public class GetLevyDeclarationQueryHandler : IRequestHandler<GetLevyDeclarationRequest, GetLevyDeclarationResponse>
    {
        private readonly IDasLevyRepository _repository;
        private readonly IValidator<GetLevyDeclarationRequest> _validator;
        private readonly IHashingService _hashingService;

        public GetLevyDeclarationQueryHandler(IDasLevyRepository repository, IValidator<GetLevyDeclarationRequest> validator, IHashingService hashingService)        
        {
            _repository = repository;
            _validator = validator;
            _hashingService = hashingService;
        }

        public async Task<GetLevyDeclarationResponse> Handle(GetLevyDeclarationRequest message,CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var accountId = _hashingService.DecodeValue(message.HashedAccountId);
            var declarations = await _repository.GetAccountLevyDeclarations(accountId);

            return new GetLevyDeclarationResponse { Declarations = declarations };
        }
    }
}
