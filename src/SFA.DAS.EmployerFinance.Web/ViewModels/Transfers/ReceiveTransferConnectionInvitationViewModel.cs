﻿using System;
using System.ComponentModel.DataAnnotations;
using SFA.DAS.Authorization.ModelBinding;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Web.Attributes;

namespace SFA.DAS.EmployerFinance.Web.ViewModels
{
    public class ReceiveTransferConnectionInvitationViewModel
    {
        [IgnoreMap]
        public string TransferConnectionInvitationId { get; set; }

        [Required(ErrorMessage = "Select an option")]
        [RegularExpression("Approve|Reject", ErrorMessage = "Select an option")]
        public string Choice { get; set; }

        public TransferConnectionInvitationDto TransferConnectionInvitation { get; set; }
        [IgnoreMap]
        public string HashedAccountId { get; set; }
        [IgnoreMap]
        public int NotHashedTransferConnectionInvitationId { get; set; }
    }
}