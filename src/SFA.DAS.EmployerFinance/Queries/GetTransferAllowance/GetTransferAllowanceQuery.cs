using System.ComponentModel.DataAnnotations;
using AutoMapper;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferAllowance;

public class GetTransferAllowanceQuery : IAuthorizationContextModel, IRequest<GetTransferAllowanceResponse>
{
    [IgnoreMap]
    [Required]
    public long AccountId { get; set; }
}