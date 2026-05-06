using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.EmployerFinance.Web.ViewModels;

public class ProviderPaymentsSummaryViewModel
{
    public string HashedAccountId { get; set; }
    public long UkPrn { get; set; }
    public string ProviderName { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    public ApprenticeshipEmployerType ApprenticeshipEmployerType { get; set; }

    public ICollection<CoursePaymentSummaryViewModel> CoursePayments { get; set; }
    public ICollection<CoursePaymentSummaryViewModel> ApprenticeshipUnitGroupSummaries { get; set; }

    public decimal LevyPaymentsTotalCourses => CoursePayments.Sum(p => p.LevyPaymentAmount);
    public decimal LevyPaymentsTotalApprenticeshipUnits => ApprenticeshipUnitGroupSummaries.Sum(p => p.LevyPaymentAmount);
    public decimal LevyPaymentsTotal => LevyPaymentsTotalCourses + LevyPaymentsTotalApprenticeshipUnits;
    public decimal SFACoInvestmentsTotalCourses => CoursePayments.Sum(p => p.SFACoInvestmentAmount);
    public decimal SFACoInvestmentsTotalApprenticeshipUnits => ApprenticeshipUnitGroupSummaries.Sum(p => p.SFACoInvestmentAmount);
    public decimal SFACoInvestmentsTotal => SFACoInvestmentsTotalCourses + SFACoInvestmentsTotalApprenticeshipUnits;
    public decimal EmployerCoInvestmentsTotalCourses => CoursePayments.Sum(p => p.EmployerCoInvestmentAmount);
    public decimal EmployerCoInvestmentsTotalApprenticeshipUnits => ApprenticeshipUnitGroupSummaries.Sum(p => p.EmployerCoInvestmentAmount);
    public decimal EmployerCoInvestmentsTotal => EmployerCoInvestmentsTotalCourses + EmployerCoInvestmentsTotalApprenticeshipUnits;
    public decimal PaymentsTotalCourses => CoursePayments.Sum(p => p.TotalAmount);
    public decimal PaymentsTotalApprenticeshipUnits => ApprenticeshipUnitGroupSummaries.Sum(p => p.TotalAmount);
    public decimal PaymentsTotal => PaymentsTotalCourses + PaymentsTotalApprenticeshipUnits;
    public bool ShowNonCoInvesmentPaymentsTotal => LevyPaymentsTotal != 0 || ApprenticeshipEmployerType == ApprenticeshipEmployerType.Levy;
}