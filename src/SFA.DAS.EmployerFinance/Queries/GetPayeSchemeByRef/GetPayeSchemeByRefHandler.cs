using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;
using SFA.DAS.HashingService;

namespace SFA.DAS.EmployerFinance.Queries.GetPayeSchemeByRef
{
    public class GetPayeSchemeByRefHandler : IRequestHandler<GetPayeSchemeByRefQuery, GetPayeSchemeByRefResponse>
    {
        private readonly IValidator<GetPayeSchemeByRefQuery> _validator;
        private readonly IPayeRepository _payeRepository;
        private readonly IEncodingService _encodingService;

        public GetPayeSchemeByRefHandler(IValidator<GetPayeSchemeByRefQuery> validator, IPayeRepository payeRepository, IEncodingService encodingService)
        {
            _validator = validator;
            _payeRepository = payeRepository;
            _encodingService = encodingService;
        }

        public async Task<GetPayeSchemeByRefResponse> Handle(GetPayeSchemeByRefQuery message,CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null,  null);
            }

            var accountId = _encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);
            var payeScheme = await _payeRepository.GetPayeForAccountByRef(accountId, message.Ref);

            return new GetPayeSchemeByRefResponse { PayeScheme = payeScheme};
        }
    }
}
