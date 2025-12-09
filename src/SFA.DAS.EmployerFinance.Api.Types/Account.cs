using System;

namespace SFA.DAS.EmployerFinance.Api.Types;

public class Account
{
    public long Id { get; protected set; }
    public string Name { get; protected set; }
    public string AccountType { get; protected set; }
    public DateTime CreatedDate { get; protected set; }
}