using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.UserAccounts;

namespace SFA.DAS.EmployerFinance.Models.UserAccounts;

public class EmployerUserAccounts
{
    public bool IsSuspended { get; set; }
    public string EmployerUserId { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public IEnumerable<EmployerUserAccountItem> EmployerAccounts { get ; set ; }

    public static implicit operator EmployerUserAccounts(GetUserAccountsResponse source)
    {
        if (source?.UserAccounts == null)
        {
            return new EmployerUserAccounts
            {
                FirstName = source?.FirstName,
                LastName = source?.LastName,
                EmployerUserId = source?.EmployerUserId,
                EmployerAccounts = new List<EmployerUserAccountItem>(),
                IsSuspended = source?.IsSuspended ?? false
            };
        }
            
        return new EmployerUserAccounts
        {
            FirstName = source.FirstName,
            LastName = source.LastName,
            EmployerUserId = source.EmployerUserId,
            EmployerAccounts = source.UserAccounts.Select(c=>(EmployerUserAccountItem)c).ToList(),
            IsSuspended = source.IsSuspended
        };
    }
}

public class EmployerUserAccountItem
{
    public string AccountId { get; set; }
    public string EmployerName { get; set; }
    public string Role { get; set; }
        
    public static implicit operator EmployerUserAccountItem(EmployerIdentifier source)
    {
        return new EmployerUserAccountItem
        {
            AccountId = source.AccountId,
            EmployerName = source.EmployerName,
            Role = source.Role
        };
    }
}