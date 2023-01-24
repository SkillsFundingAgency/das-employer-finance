using System;
using MediatR;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Commands.CreateTransferTransactions
{
    public class CreateTransferTransactionsCommand : IAuthorizationContextModel,IRequest<Unit>
    {
        public long ReceiverAccountId { get; set; }
        public string PeriodEnd { get; set; }
        public Guid CorrelationId { get; set; }
    }
}
