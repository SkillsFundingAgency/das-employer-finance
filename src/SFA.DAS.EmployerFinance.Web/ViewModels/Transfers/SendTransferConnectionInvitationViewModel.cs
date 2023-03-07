using System;
using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Web.Attributes;

namespace SFA.DAS.EmployerFinance.Web.ViewModels
{
    public class SendTransferConnectionInvitationViewModel
    {
        [Required(ErrorMessage = "Option required")]
        [RegularExpression("Confirm|ReEnterAccountId", ErrorMessage = "Option required")]
        public string Choice { get; set; }

        public AccountDto ReceiverAccount { get; set; }
        public AccountDto SenderAccount { get; set; }

        [Required]
        [RegularExpression(EmployerFinance.Constants.AccountHashedIdRegex)]
        public string ReceiverAccountPublicHashedId { get; set; }

        [IgnoreMap]
        public string HashedAccountId { get; set; }
    }
}