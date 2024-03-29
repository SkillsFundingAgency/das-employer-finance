﻿using SFA.DAS.EmployerFinance.Models.Transfers;

namespace SFA.DAS.EmployerFinance.Data.Contracts;

public interface ITransferRepository
{
    Task CreateAccountTransfers(IEnumerable<AccountTransfer> transfers);
    Task<IEnumerable<AccountTransfer>> GetReceiverAccountTransfersByPeriodEnd(long receiverAccountId, string periodEnd);
    Task<AccountTransferDetails> GetTransferPaymentDetails(AccountTransfer transfer);
    Task<TransferAllowance> GetTransferAllowance(long accountId, decimal transferAllowancePercentage);
}