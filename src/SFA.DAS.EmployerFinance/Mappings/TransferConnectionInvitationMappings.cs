﻿using AutoMapper;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Models.TransferConnections;

namespace SFA.DAS.EmployerFinance.Mappings;

public class TransferConnectionInvitationMappings : Profile
{
    public TransferConnectionInvitationMappings()
    {
        CreateMap<TransferConnectionInvitation, TransferConnectionInvitationDto>()
            .ForMember(m => m.Changes, o => o.MapFrom(i => i.Changes.OrderBy(c => c.CreatedDate)));

        CreateMap<TransferConnectionInvitationChange, TransferConnectionInvitationChangeDto>();

        CreateMap<TransferConnectionInvitation, TransferConnection>()
            .ForMember(m => m.FundingEmployerAccountId, o => o.MapFrom(i => i.SenderAccount.Id))
            .ForMember(m => m.FundingEmployerHashedAccountId, o => o.MapFrom(i => i.SenderAccount.HashedId))
            .ForMember(m => m.FundingEmployerPublicHashedAccountId, o => o.MapFrom(i => i.SenderAccount.PublicHashedId))
            .ForMember(m => m.FundingEmployerAccountName, o => o.MapFrom(i => i.SenderAccount.Name))
            .ForMember(m => m.FundingEmployerAccountName, o => o.MapFrom(i => i.SenderAccount.Name))
            .ForMember(m => m.Status, o => o.MapFrom(i => (short?)i.Status))
            .ForMember(dest => dest.StatusSetOn,
                opt => opt.MapFrom(src =>
                    src.Changes.Where(s => s.Status == src.Status).Select(s => (DateTime?)s.CreatedDate).FirstOrDefault()));
    }
}




