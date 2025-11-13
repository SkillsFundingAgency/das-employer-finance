using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Api.Mappings;

public class AccountMappings : Profile
{
    public AccountMappings()
    {            
        CreateMap<LevyDeclarationItem, LevyDeclaration>()
            .ForMember(target => target.PayeSchemeReference, opt => opt.MapFrom(src => src.EmpRef));

        CreateMap<Models.Account.AccountBalance, AccountBalance>();
        CreateMap<Models.Transfers.TransferAllowance, TransferAllowance>();

        CreateMap<Models.Account.Account, Account>();

        CreateMap<Account, Models.Account.Account>()
            .ForMember(dest => dest.SentTransferConnectionInvitations,
                opt => opt.MapFrom(src => src.SentTransferConnectionInvitations))
            .ForMember(dest => dest.ReceivedTransferConnectionInvitations,
                opt => opt.MapFrom(src => src.ReceivedTransferConnectionInvitations));

        //CreateMap<TransferConnectionInvitation, Models.Transfers.TransferConnectionInvitation>()
        //    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
        //    .ForMember(dest => dest.SenderAccountId, opt => opt.MapFrom(src => src.SenderAccount.Id))
        //    .ForMember(dest => dest.SenderAccountName, opt => opt.MapFrom(src => src.SenderAccount.Name))
        //    .ForMember(dest => dest.ReceiverAccountId, opt => opt.MapFrom(src => src.ReceiverAccount.Id))
        //    .ForMember(dest => dest.ReceiverAccountName, opt => opt.MapFrom(src => src.ReceiverAccount.Name))
        //    .ForMember(dest => dest.Changes, opt => opt.MapFrom(src => src.Changes));

        //CreateMap<TransferConnectionInvitationChange, Models.Transfers.TransferConnectionInvitationChange>()
        //    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
        //    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id))
        //    .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName));
    }
}