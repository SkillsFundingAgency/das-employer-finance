using System.ComponentModel.DataAnnotations;
using AutoMapper.Configuration.Annotations;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferRequests;

public class GetTransferRequestsQuery : IAuthorizationContextModel, IRequest<GetTransferRequestsResponse>
{
    [Ignore]
    [Required]
    public long AccountId { get; set; }
}