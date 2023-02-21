using System.ComponentModel.DataAnnotations;
using AutoMapper.Configuration.Annotations;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;

public class GetAccountFinanceOverviewQuery : IAuthorizationContextModel, IRequest<GetAccountFinanceOverviewResponse>
{
    [Ignore]
    [Required]
    public long AccountId { get; set; }

    [Ignore]
    [Required]
    public string AccountHashedId { get; set; }
}