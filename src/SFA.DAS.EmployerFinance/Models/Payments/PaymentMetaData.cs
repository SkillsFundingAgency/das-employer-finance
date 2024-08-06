namespace SFA.DAS.EmployerFinance.Models.Payments;

public class PaymentMetaData : Entity
{
    public long Id { get; set; }
    public string ProviderName { get; set; }
    public long? StandardCode { get; set; }
    public int? FrameworkCode { get; set; }
    public int? ProgrammeType { get; set; }
    public int? PathwayCode { get; set; }
    public string PathwayName { get; set; }
    public string ApprenticeshipCourseName { get; set; }
    public DateTime? ApprenticeshipCourseStartDate { get; set; }
    public int? ApprenticeshipCourseLevel { get; set; }
    public string ApprenticeName { get; set; }
    public string ApprenticeNINumber { get; set; }
    public bool IsHistoricProviderName { get; set; }
}