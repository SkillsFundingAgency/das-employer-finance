﻿using System.ComponentModel.DataAnnotations;
using AutoMapper;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Queries.GetApprovedTransferConnectionInvitation;

public class GetApprovedTransferConnectionInvitationQuery : IAuthorizationContextModel, IRequest<GetApprovedTransferConnectionInvitationResponse>
{
    [IgnoreMap]
    [Required]
    public long AccountId { get; set; }

    [Required]
    public int? TransferConnectionInvitationId { get; set; }
}