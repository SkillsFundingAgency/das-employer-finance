using SFA.DAS.EmployerFinance.Models.TransferConnections;

namespace SFA.DAS.EmployerFinance.Data.Contracts;

public interface ITransferConnectionInvitationRepository
{
    Task Add(TransferConnectionInvitation transferConnectionInvitation);
    Task<TransferConnectionInvitation> Get(int id);
    Task<TransferConnectionInvitation> GetBySender(int id, long senderAccountId, TransferConnectionInvitationStatus status);
    Task<TransferConnectionInvitation> GetByReceiver(int id, long receiverAccountId, TransferConnectionInvitationStatus status);
    Task<List<TransferConnectionInvitation>> GetByReceiver(long receiverAccountId, TransferConnectionInvitationStatus status);
    Task<TransferConnectionInvitation> GetLatestByReceiver(long receiverAccountId, TransferConnectionInvitationStatus status);
    Task<TransferConnectionInvitation> GetBySenderOrReceiver(int id, long accountId);
    Task<List<TransferConnectionInvitation>> GetBySenderOrReceiver(long accountId);
    Task<bool> AnyTransferConnectionInvitations(long senderAccountId, long receiverAccountId, List<TransferConnectionInvitationStatus> statuses);
}