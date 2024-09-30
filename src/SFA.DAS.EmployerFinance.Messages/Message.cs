using System;

namespace SFA.DAS.EmployerFinance.Messages;

public abstract class Message
{
    public DateTime Created { get; set; }
}