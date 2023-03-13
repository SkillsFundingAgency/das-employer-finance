
namespace SFA.DAS.EmployerFinance.Commands.CreateTransferTransactions;

public class CreateTransferTransactionsCommand : IRequest<Unit>
{
    public long ReceiverAccountId { get; set; }
    public string PeriodEnd { get; set; }
    public Guid CorrelationId { get; set; }
}