﻿using AutoMapper;
using SFA.DAS.ReferenceData.Types.DTO;

namespace SFA.DAS.EAS.Infrastructure.Mapping.Profiles
{
    public class ReferenceDataMappings : Profile
    {
        public ReferenceDataMappings()
        {
            CreateMap<Charity, Domain.Models.ReferenceData.Charity>();
            CreateMap<PublicSectorOrganisation, Domain.Models.ReferenceData.PublicSectorOrganisation>();
        }
    }
}
