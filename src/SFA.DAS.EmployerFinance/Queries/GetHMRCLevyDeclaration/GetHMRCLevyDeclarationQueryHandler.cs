﻿using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Validation;
using SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclaration;
using SFA.DAS.Hmrc;
using System.Threading;

namespace SFA.DAS.EmployerFinance.Queries.GetHMRCLevyDeclaration
{
    public class GetHMRCLevyDeclarationQueryHandler : IRequestHandler<GetHMRCLevyDeclarationQuery, GetHMRCLevyDeclarationResponse>
    {
        private readonly IValidator<GetHMRCLevyDeclarationQuery> _validator;
        private readonly IHmrcService _hmrcService;
        private readonly IMediator _mediator;

        public GetHMRCLevyDeclarationQueryHandler(IValidator<GetHMRCLevyDeclarationQuery> validator, IHmrcService hmrcService, IMediator mediator)
        {
            _validator = validator;
            _hmrcService = hmrcService;
            _mediator = mediator;
        }

        public async Task<GetHMRCLevyDeclarationResponse> Handle(GetHMRCLevyDeclarationQuery message,CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid())
            {
                throw new InvalidRequestException(validationResult.ValidationDictionary);
            }

            var existingDeclaration = await _mediator.Send(new GetLastLevyDeclarationQuery { EmpRef = message.EmpRef });
            
            DateTime? dateFrom = null;
            if (existingDeclaration?.Transaction?.SubmissionDate != null && existingDeclaration.Transaction.SubmissionDate != DateTime.MinValue)
            {
                dateFrom = existingDeclaration.Transaction?.SubmissionDate.AddDays(-1);
            }

            var declarations = await _hmrcService.GetLevyDeclarations(message.EmpRef, dateFrom);
            
            var getLevyDeclarationResponse = new GetHMRCLevyDeclarationResponse
            {
                LevyDeclarations = declarations,
                Empref = message.EmpRef
            };

            return getLevyDeclarationResponse;
        }
    }
}
