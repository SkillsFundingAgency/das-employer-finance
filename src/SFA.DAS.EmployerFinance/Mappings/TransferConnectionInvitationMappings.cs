using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Models.TransferConnections;

namespace SFA.DAS.EmployerFinance.Mappings;

public class TransferConnectionInvitationMappings : Profile
{
    public TransferConnectionInvitationMappings()
    {
        CreateMap<Models.TransferConnections.TransferConnectionInvitation, TransferConnectionInvitationDto>()
            .ForMember(m => m.Changes, o => o.MapFrom(i => i.Changes.OrderBy(c => c.CreatedDate)));

        CreateMap<Models.TransferConnections.TransferConnectionInvitationChange, TransferConnectionInvitationChangeDto>();

        CreateMap<Models.TransferConnections.TransferConnectionInvitation, TransferConnection>()
            .ForMember(m => m.FundingEmployerAccountId, o => o.MapFrom(i => i.SenderAccount.Id))
            .ForMember(m => m.FundingEmployerHashedAccountId, o => o.MapFrom(i => i.SenderAccount.HashedId))
            .ForMember(m => m.FundingEmployerPublicHashedAccountId, o => o.MapFrom(i => i.SenderAccount.PublicHashedId))
            .ForMember(m => m.FundingEmployerAccountName, o => o.MapFrom(i => i.SenderAccount.Name))
            .ForMember(m => m.Status, o => o.MapFrom(i => (short?)i.Status))
            .ForMember(dest => dest.StatusAssignedOn,
                opt => opt.MapFrom(src =>
                    src.Changes.Where(s => s.Status == src.Status).Select(s => (DateTime?)s.CreatedDate).FirstOrDefault()));

        CreateMap<Models.TransferConnections.TransferConnectionInvitation, Api.Types.TransferConnectionInvitation>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.SenderAccountId, opt => opt.MapFrom(src => src.SenderAccount.Id))
            .ForMember(dest => dest.SenderAccountName, opt => opt.MapFrom(src => src.SenderAccount.Name))
            .ForMember(dest => dest.ReceiverAccountId, opt => opt.MapFrom(src => src.ReceiverAccount.Id))
            .ForMember(dest => dest.ReceiverAccountName, opt => opt.MapFrom(src => src.ReceiverAccount.Name))
            .ForMember(dest => dest.Changes, opt => opt.MapFrom(src => src.Changes));

        CreateMap<Models.TransferConnections.TransferConnectionInvitationChange, Api.Types.TransferConnectionInvitationChange>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName));
    }
}