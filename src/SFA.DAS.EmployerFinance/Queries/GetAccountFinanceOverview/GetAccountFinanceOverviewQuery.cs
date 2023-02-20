using System.ComponentModel.DataAnnotations;
using AutoMapper;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;

public class GetAccountFinanceOverviewQuery : IAuthorizationContextModel, IRequest<GetAccountFinanceOverviewResponse>
{
    [IgnoreMap]
    [Required]
    public long AccountId { get; set; }

    [IgnoreMap]
    [Required]
    public string AccountHashedId { get; set; }
}