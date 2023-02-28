using System.ComponentModel.DataAnnotations;
using AutoMapper.Configuration.Annotations;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferAllowance;

public class GetTransferAllowanceQuery : IRequest<GetTransferAllowanceResponse>
{
    [Ignore]
    [Required]
    public long AccountId { get; set; }
}