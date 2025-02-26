﻿using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.TransferConnections;

namespace SFA.DAS.EmployerFinance.Data;

public class TransferConnectionInvitationRepository(Lazy<EmployerFinanceDbContext> db) : ITransferConnectionInvitationRepository
{
    public Task Add(TransferConnectionInvitation transferConnectionInvitation)
    {
        db.Value.Attach(transferConnectionInvitation);
        db.Value.TransferConnectionInvitations.Add(transferConnectionInvitation);

        // TODO: investigate why this is saved here - shouldn't the unit of work pattern be saving it with the published events
        return db.Value.SaveChangesAsync();
    }

    public Task<TransferConnectionInvitation> Get(int id)
    {
        return db.Value.TransferConnectionInvitations
            .Include(i => i.ReceiverAccount)
            .Include(i => i.SenderAccount)
            .Include(c => c.Changes)
            .ThenInclude(x => x.User)
            .Where(i => i.Id == id)
            .SingleOrDefaultAsync();
    }

    public Task<TransferConnectionInvitation> GetBySender(int id, long senderAccountId, TransferConnectionInvitationStatus status)
    {
        var query = db.Value.TransferConnectionInvitations
            .Include(c => c.ReceiverAccount)
            .Include(c => c.SenderAccount)
            .Include(c => c.Changes)
            .ThenInclude(x => x.User)
            .Where(i =>
                i.Id == id &&
                i.SenderAccount.Id == senderAccountId &&
                i.Status == status);

        return query
            .SingleOrDefaultAsync();
    }

    public Task<TransferConnectionInvitation> GetByReceiver(int id, long receiverAccountId, TransferConnectionInvitationStatus status)
    {
        var query = db.Value.TransferConnectionInvitations
            .Include(c => c.ReceiverAccount)
            .Include(c => c.SenderAccount)
            .Include(c => c.Changes)
            .ThenInclude(x => x.User)
            .Where(i =>
                i.Id == id &&
                i.ReceiverAccount.Id == receiverAccountId &&
                i.Status == status);

        return query
            .SingleOrDefaultAsync();
    }

    public Task<List<TransferConnectionInvitation>> GetByReceiver(long receiverAccountId, TransferConnectionInvitationStatus status)
    {
        return db.Value.TransferConnectionInvitations
            .Include(c => c.ReceiverAccount)
            .Include(c => c.SenderAccount)
            .Include(c => c.Changes)
            .ThenInclude(x => x.User)
            .Where(
                i => i.ReceiverAccount.Id == receiverAccountId &&
                     i.Status == status)
            .OrderBy(i => i.SenderAccount.Name)
            .ToListAsync();
    }

    public Task<TransferConnectionInvitation> GetLatestByReceiver(long receiverAccountId, TransferConnectionInvitationStatus status)
    {
        return db.Value.TransferConnectionInvitations
            .Include(c => c.ReceiverAccount)
            .Include(c => c.SenderAccount)
            .Include(c => c.Changes)
            .ThenInclude(x => x.User)
            .Where(
                i => i.ReceiverAccountId == receiverAccountId &&
                     i.Status == status)
            .OrderByDescending(i => i.CreatedDate)
            .FirstOrDefaultAsync();
    }

    public Task<TransferConnectionInvitation> GetBySenderOrReceiver(int id, long accountId)
    {
        return db.Value.TransferConnectionInvitations
            .Include(c => c.ReceiverAccount)
            .Include(c => c.SenderAccount)
            .Include(c => c.Changes)
            .ThenInclude(c => c.User)
            .Where(i =>
                i.Id == id && (
                    i.SenderAccount.Id == accountId && !i.DeletedBySender ||
                    i.ReceiverAccount.Id == accountId))
            .SingleOrDefaultAsync();
    }

    public Task<List<TransferConnectionInvitation>> GetBySenderOrReceiver(long accountId)
    {
        var query = db.Value.TransferConnectionInvitations
            .Include(i => i.ReceiverAccount)
            .Include(i => i.SenderAccount)
            .Include(x => x.Changes)
            .ThenInclude(x => x.User)
            .Where(
                i => i.SenderAccount.Id == accountId && !i.DeletedBySender ||
                     i.ReceiverAccount.Id == accountId && !i.DeletedByReceiver)
            .OrderBy(i => i.ReceiverAccount.Id == accountId ? i.SenderAccount.Name : i.ReceiverAccount.Name)
            .ThenBy(i => i.CreatedDate);

        return query
            .ToListAsync();
    }

    public Task<bool> AnyTransferConnectionInvitations(long senderAccountId, long receiverAccountId, List<TransferConnectionInvitationStatus> statuses)
    {
        return db.Value.TransferConnectionInvitations.AnyAsync(i =>
            i.SenderAccount.Id == senderAccountId &&
            i.ReceiverAccount.Id == receiverAccountId &&
            statuses.Contains(i.Status));
    }
}