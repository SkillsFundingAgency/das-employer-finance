using System.ComponentModel.DataAnnotations;
using MediatR;
using SFA.DAS.EmployerFinance.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.GetLevyDeclaration
{
    public class GetLevyDeclarationQueryHandler : IRequestHandler<GetLevyDeclarationRequest, GetLevyDeclarationResponse>
    {
        private readonly IDasLevyRepository _repository;
        private readonly IValidator<GetLevyDeclarationRequest> _validator;
        private readonly IEncodingService _encodingService;

        public GetLevyDeclarationQueryHandler(IDasLevyRepository repository, IValidator<GetLevyDeclarationRequest> validator, IEncodingService encodingService)        
        {
            _repository = repository;
            _validator = validator;
            _encodingService = encodingService;
        }

        public async Task<GetLevyDeclarationResponse> Handle(GetLevyDeclarationRequest message,CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            var accountId = _encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);
            var declarations = await _repository.GetAccountLevyDeclarations(accountId);

            return new GetLevyDeclarationResponse { Declarations = declarations };
        }
    }
}
