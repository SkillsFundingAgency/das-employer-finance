using System.ComponentModel.DataAnnotations;
using AutoMapper.Configuration.Annotations;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferAllowance;

public class GetTransferAllowanceQuery : IAuthorizationContextModel, IRequest<GetTransferAllowanceResponse>
{
    [Ignore]
    [Required]
    public long AccountId { get; set; }
}