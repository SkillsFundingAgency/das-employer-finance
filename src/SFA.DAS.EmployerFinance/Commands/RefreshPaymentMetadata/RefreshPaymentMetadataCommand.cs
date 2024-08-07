﻿
namespace SFA.DAS.EmployerFinance.Commands.RefreshPaymentMetadata;

public class RefreshPaymentMetadataCommand : IRequest
{
    public string PeriodEndRef { get; init; }
    public long AccountId { get; init; }
    public Guid PaymentId { get; set; }
}