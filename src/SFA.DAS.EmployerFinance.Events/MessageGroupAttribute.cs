using System;

namespace SFA.DAS.EmployerFinance.Events;

public class MessageGroupAttribute : Attribute
{
    public MessageGroupAttribute(string name = "") { }
}