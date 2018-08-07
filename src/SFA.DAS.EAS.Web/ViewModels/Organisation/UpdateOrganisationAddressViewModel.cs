﻿using System;

namespace SFA.DAS.EAS.Web.ViewModels.Organisation
{

    [Flags]
    public enum OrganisationUpdatesAvailable
    {
        None = 0,
        Name = 1,
        Address = 2,
        Any = Name | Address
    }

    public class ReviewOrganisationAddressViewModel : OrganisationViewModelBase
    {
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string RefreshedName { get; set; }
        public string RefreshedAddress { get; set; }
        public OrganisationUpdatesAvailable UpdatesAvailable { get; set; }
    }
}