using System;

namespace SFA.DAS.EmployerFinance.Web.Attributes
{
    // Summary:
    // Ignore this member for validation and skip during mapping
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class IgnoreMapAttribute : Attribute
    {
    }
}