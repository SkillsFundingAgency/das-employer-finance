using System.ComponentModel.DataAnnotations;
using AutoMapper.Configuration.Annotations;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferTransactionDetails;

public class GetTransferTransactionDetailsQuery : IRequest<GetTransferTransactionDetailsResponse>
{
    [Ignore]
    [Required]
    public long? AccountId { get; set; }

    [Required]
    public string TargetAccountPublicHashedId { get; set; }

    [Required]
    public string PeriodEnd { get; set; }
}