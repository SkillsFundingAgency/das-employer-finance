using System;
using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Web.Attributes;

namespace SFA.DAS.EmployerFinance.Web.ViewModels
{
    public class StartTransferConnectionInvitationViewModel 
    {
        [IgnoreMap]
        public string HashedAccountId { get; set; }

        [IgnoreMap]
        [Required]
        public Guid UserRef { get; set; }

        [Required(ErrorMessage = "You must enter a valid account ID")]
        [RegularExpression(EmployerFinance.Constants.AccountHashedIdRegex, ErrorMessage = "You must enter a valid account ID")]
        public string ReceiverAccountPublicHashedId { get; set; }
    }
}