﻿
using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.EmployerFinance.Web.ViewModels
{
    public class TransferConnectionInvitationAuthorizationViewModel
    {
        public bool AuthorizationResult { get; set; }//TODO MAC-192
        public bool IsValidSender { get; set; }
        public decimal TransferAllowancePercentage { get => _TransferAllowancePercentage * 100; set => _TransferAllowancePercentage = value; }
        public string HashedAccountId { get; set; }

        private decimal _TransferAllowancePercentage;
    }
}