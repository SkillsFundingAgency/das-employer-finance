﻿using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Queries.FindAccountCoursePayments;

public class FindAccountCoursePaymentsResponse
{
    public string ProviderName { get; set; }
    public string CourseName { get; set; }
    public int? CourseLevel { get; set; }
    public string PathwayName { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime DateCreated { get; set; }
    public List<PaymentTransactionLine> Transactions { get; set; }
    public decimal Total { get; set; }
}