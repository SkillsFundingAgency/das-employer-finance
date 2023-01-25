﻿using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MediatR;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitations
{
    public class GetTransferConnectionInvitationsQuery : IAuthorizationContextModel, IRequest<GetTransferConnectionInvitationsResponse>
    {
        [IgnoreMap]
        [Required]
        public long AccountId { get; set; }
    }
}