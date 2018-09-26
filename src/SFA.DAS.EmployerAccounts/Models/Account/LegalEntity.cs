﻿using System;
using System.Collections.Generic;
using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.EmployerAccounts.Models.Account
{
    public class LegalEntity
    {
        public virtual long Id { get; set; }
        public virtual ICollection<EmployerAgreement> Agreements { get; set; } = new List<EmployerAgreement>();
        public virtual string Code { get; set; }
        public virtual DateTime? DateOfIncorporation { get; set; }
        public virtual string Name { get; set; }
        public virtual byte? PublicSectorDataSource { get; set; }
        public virtual string RegisteredAddress { get; set; }
        public virtual string Sector { get; set; }
        public virtual OrganisationType Source { get; set; }
        public virtual string Status { get; set; }
    }
}