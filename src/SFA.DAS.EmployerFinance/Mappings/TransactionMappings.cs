using AutoMapper;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Models.Transfers;

namespace SFA.DAS.EmployerFinance.Mappings;

public class TransactionMappings : Profile
{
    public TransactionMappings()
    {
        CreateMap<TransactionEntity, TransactionLine>()
            .ForMember(d => d.Description, o => o.Ignore())
            .ForMember(d => d.SubTransactions, o => o.Ignore())
            .ForMember(d => d.PayrollDate, o => o.Ignore());

        CreateMap<TransactionEntity, PaymentTransactionLine>()
            .ForMember(d => d.PaymentId, o => o.Ignore())
            .ForMember(d => d.Description, o => o.Ignore())
            .ForMember(d => d.SubTransactions, o => o.Ignore())
            .ForMember(d => d.PayrollDate, o => o.Ignore())
            .ForMember(d => d.LearningType, 
                o => 
                    o.MapFrom(c => c.LearningType != null ? Enum.Parse<LearningType>(c.LearningType) : LearningType.Apprenticeship));

        CreateMap<TransactionEntity, LevyDeclarationTransactionLine>()
            .ForMember(d => d.PayeSchemeName, o => o.Ignore())
            .ForMember(d => d.LineTotal, o => o.Ignore())
            .ForMember(d => d.Description, o => o.Ignore())
            .ForMember(d => d.SubTransactions, o => o.Ignore())
            .ForMember(d => d.PayrollDate, o => o.Ignore());

        CreateMap<TransactionEntity, TransferTransactionLine>()
            .ForMember(d => d.ReceiverAccountPublicHashedId, o => o.Ignore())
            .ForMember(d => d.SenderAccountPublicHashedId, o => o.Ignore())
            .ForMember(d => d.Description, o => o.Ignore())
            .ForMember(d => d.SubTransactions, o => o.Ignore())
            .ForMember(d => d.PayrollDate, o => o.Ignore());

        CreateMap<TransactionEntity, ExpiredFundTransactionLine>()
            .ForMember(d => d.Description, o => o.Ignore())
            .ForMember(d => d.SubTransactions, o => o.Ignore())
            .ForMember(d => d.PayrollDate, o => o.Ignore());

        CreateMap<TransactionLineEntity, TransactionLine>()
            .ForMember(d => d.Description, o => o.Ignore())
            .ForMember(d => d.SubTransactions, o => o.Ignore())
            .ForMember(d => d.PayrollDate, o => o.Ignore())
            .ForMember(d => d.PayrollYear, o => o.Ignore())
            .ForMember(d => d.PayrollMonth, o => o.Ignore())
            .ForMember(d => d.Balance, o => o.Ignore());

        CreateMap<TransactionLineEntity, PaymentTransactionLine>()
            .IncludeBase<TransactionLineEntity, TransactionLine>()
            .ForMember(d => d.PaymentId, o => o.Ignore())
            .ForMember(d => d.UkPrn, o => o.MapFrom(s => s.UkPrn ?? 0))
            .ForMember(d => d.ProviderName, o => o.Ignore())
            .ForMember(d => d.LineAmount, o => o.Ignore())
            .ForMember(d => d.CourseName, o => o.Ignore())
            .ForMember(d => d.CourseLevel, o => o.Ignore())
            .ForMember(d => d.PathwayName, o => o.Ignore())
            .ForMember(d => d.PathwayCode, o => o.Ignore())
            .ForMember(d => d.CourseStartDate, o => o.Ignore())
            .ForMember(d => d.ApprenticeName, o => o.Ignore())
            .ForMember(d => d.ApprenticeULN, o => o.Ignore())
            .ForMember(d => d.ApprenticeNINumber, o => o.Ignore())
            .ForMember(d => d.LearningType, o => o.Ignore());

        CreateMap<TransactionLineEntity, LevyDeclarationTransactionLine>()
            .IncludeBase<TransactionLineEntity, TransactionLine>()
            .ForMember(d => d.SubmissionId, o => o.MapFrom(s => s.SubmissionId ?? 0))
            .ForMember(d => d.EnglishFraction, o => o.MapFrom(s => s.EnglishFraction ?? 0))
            .ForMember(d => d.PayeSchemeName, o => o.Ignore())
            .ForMember(d => d.TopUp, o => o.Ignore())
            .ForMember(d => d.LineTotal, o => o.Ignore())
            .ForMember(d => d.LineAmount, o => o.Ignore());

        CreateMap<TransactionLineEntity, TransferTransactionLine>()
            .IncludeBase<TransactionLineEntity, TransactionLine>()
            .ForMember(d => d.SenderAccountId, o => o.MapFrom(s => s.TransferSenderAccountId ?? 0))
            .ForMember(d => d.SenderAccountName, o => o.MapFrom(s => s.TransferSenderAccountName))
            .ForMember(d => d.ReceiverAccountId, o => o.MapFrom(s => s.TransferReceiverAccountId ?? 0))
            .ForMember(d => d.ReceiverAccountName, o => o.MapFrom(s => s.TransferReceiverAccountName))
            .ForMember(d => d.ReceiverAccountPublicHashedId, o => o.Ignore())
            .ForMember(d => d.SenderAccountPublicHashedId, o => o.Ignore());

        CreateMap<TransactionLineEntity, ExpiredFundTransactionLine>()
            .IncludeBase<TransactionLineEntity, TransactionLine>();
    }
}
