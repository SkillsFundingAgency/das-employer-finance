using System;
using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Web.Attributes;

namespace SFA.DAS.EmployerFinance.Web.ViewModels
{
    public class SendTransferConnectionInvitationViewModel
    {
        [Required(ErrorMessage = "Select an option")]
        [RegularExpression("Confirm|ReEnterAccountId", ErrorMessage = "Select an option")]
        public string Choice { get; set; }

        [Required]
        [RegularExpression(EmployerFinance.Constants.AccountHashedIdRegex)]
        public string ReceiverAccountPublicHashedId { get; set; }

        [IgnoreMap]
        public string HashedAccountId { get; set; }

        public string SenderAccountName { get; set; }
        public string ReceiverAccountName { get; set; }
    }
}