﻿using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.HashingService;

namespace SFA.DAS.EmployerFinance.Queries.GetPayeSchemeByRef
{
    public class GetPayeSchemeByRefHandler : IRequestHandler<GetPayeSchemeByRefQuery, GetPayeSchemeByRefResponse>
    {
        private readonly IValidator<GetPayeSchemeByRefQuery> _validator;
        private readonly IPayeRepository _payeRepository;
        private readonly IHashingService _hashingService;

        public GetPayeSchemeByRefHandler(IValidator<GetPayeSchemeByRefQuery> validator, IPayeRepository payeRepository, IHashingService hashingService)
        {
            _validator = validator;
            _payeRepository = payeRepository;
            _hashingService = hashingService;
        }

        public async Task<GetPayeSchemeByRefResponse> Handle(GetPayeSchemeByRefQuery message,CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null,  null);
            }

            var accountId = _hashingService.DecodeValue(message.HashedAccountId);
            var payeScheme = await _payeRepository.GetPayeForAccountByRef(accountId, message.Ref);

            return new GetPayeSchemeByRefResponse { PayeScheme = payeScheme};
        }
    }
}
