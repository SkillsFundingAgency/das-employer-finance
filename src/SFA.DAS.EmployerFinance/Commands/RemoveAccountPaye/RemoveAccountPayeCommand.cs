﻿namespace SFA.DAS.EmployerFinance.Commands.RemoveAccountPaye;

public class RemoveAccountPayeCommand : IRequest
{
    public long AccountId { get; }
    public string PayeRef { get; }

    public RemoveAccountPayeCommand(long accountId, string payeRef)
    {
        AccountId = accountId;
        PayeRef = payeRef;
    }
}