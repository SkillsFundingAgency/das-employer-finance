﻿using AutoMapper;
using SFA.DAS.EAS.Domain.Models.ExpiredFunds;
using SFA.DAS.EAS.Domain.Models.Transaction;
using SFA.DAS.EAS.Domain.Models.Transfers;

namespace SFA.DAS.EAS.Infrastructure.Mapping.Profiles
{
    public class TransactionMappings : Profile
    {
        public TransactionMappings()
        {
            CreateMap<TransactionEntity, TransactionLine>();
            CreateMap<TransactionEntity, TransferTransactionLine>();
            CreateMap<TransactionEntity, ExpiredFundTransactionLine>();
        }
    }
}
