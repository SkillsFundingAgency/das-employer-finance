using System;
using MediatR;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Commands.RefreshPaymentData
{
    public class RefreshPaymentDataCommand : IAuthorizationContextModel,IRequest<Unit>
    {
        public long AccountId { get; set; }
        public string PeriodEnd { get; set; }
        public Guid CorrelationId { get; set; }
    }
}
