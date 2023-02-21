using System.ComponentModel.DataAnnotations;
using AutoMapper.Configuration.Annotations;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferTransactionDetails;

public class GetTransferTransactionDetailsQuery : IAuthorizationContextModel, IRequest<GetTransferTransactionDetailsResponse>
{
    [Ignore]
    [Required]
    public long? AccountId { get; set; }

    [Required]
    public string TargetAccountPublicHashedId { get; set; }

    [Required]
    public string PeriodEnd { get; set; }
}