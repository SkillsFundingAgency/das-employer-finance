﻿namespace SFA.DAS.EmployerFinance.Commands.RemoveAccountLegalEntity;

public class RemoveAccountLegalEntityCommand : IRequest
{
    public RemoveAccountLegalEntityCommand(long id)
    {
        Id = id;
    }

    public long Id { get; set; }
}