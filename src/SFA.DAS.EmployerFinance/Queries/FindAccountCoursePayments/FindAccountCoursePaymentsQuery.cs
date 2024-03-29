namespace SFA.DAS.EmployerFinance.Queries.FindAccountCoursePayments;

public class FindAccountCoursePaymentsQuery : IRequest<FindAccountCoursePaymentsResponse>
{
    public string HashedAccountId { get; set; }
    public long UkPrn { get; set; }
    public string CourseName { get; set; }
    public int? CourseLevel { get; set; }
    public int? PathwayCode { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}