namespace SFA.DAS.EmployerFinance.Web.ViewModels;

public class CoursePaymentsSummaryPartialViewModel(
    CoursePaymentSummaryViewModel coursePaymentSummaryViewModel,
    string hashedAccountId,
    long ukPrn,
    DateTime fromDate,
    DateTime toDate,
    bool showNonCoInvesmentPaymentsTotal)
{
    public CoursePaymentSummaryViewModel CoursePaymentSummaryViewModel { get; set; } = coursePaymentSummaryViewModel;
    public string HashedAccountId { get; set; } = hashedAccountId;
    public long UkPrn { get; set; } = ukPrn;
    public DateTime FromDate { get; set; } = fromDate;
    public DateTime ToDate { get; set; } = toDate;
    public bool ShowNonCoInvesmentPaymentsTotal { get; set; } = showNonCoInvesmentPaymentsTotal;
}