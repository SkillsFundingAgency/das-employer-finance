﻿using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.EmployerFinance.Web.ViewModels;

public class CoursePaymentDetailsViewModel
{
    public string CourseName { get; set; }
    public int? CourseLevel { get; set; }
    public string PathwayName { get; set; }
    public string ProviderName { get; set; }
    public DateTime PaymentDate { get; set; }

    public decimal LevyPaymentsTotal { get; set; }
    public decimal SFACoInvestmentTotal { get; set; }
    public decimal EmployerCoInvestmentTotal { get; set; }

    public decimal PaymentsTotal => LevyPaymentsTotal + SFACoInvestmentTotal + EmployerCoInvestmentTotal;

    public ICollection<AprrenticeshipPaymentSummaryViewModel> ApprenticePayments { get; set; }
    public string HashedAccountId { get; set; }
    public ApprenticeshipEmployerType ApprenticeshipEmployerType { get; set; }

    public bool ShowNonCoInvesmentPaymentsTotal => LevyPaymentsTotal != 0 
                                                   || ApprenticeshipEmployerType == ApprenticeshipEmployerType.Levy;
}