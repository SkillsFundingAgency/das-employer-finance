using SFA.DAS.EmployerFinance.Models;

namespace SFA.DAS.EmployerFinance.Interfaces;

/// <summary>
/// Defines a rule for invalidating cached items.
/// </summary>
/// <typeparam name="T">The type of the cached item.</typeparam>
public interface ICacheInvalidationRule<in T> where T : class
{
    Task<bool> ShouldInvalidateAsync(T cached, CacheInvalidationContext context);
}