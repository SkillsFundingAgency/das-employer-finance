using System;

namespace SFA.DAS.EmployerFinance.Api.Types;

public class Payment
{
    public string ProviderName { get; set; } = string.Empty;

    public int StandardCode { get; set; }

    public int FrameworkCode { get; set; }

    public int ProgrammeType { get; set; }

    public int PathwayCode { get; set; }

    public string PathwayName { get; set; } = string.Empty;

    public string ApprenticeshipCourseName { get; set; } = string.Empty;

    public string ApprenticeName { get; set; } = string.Empty;

    public string ApprenticeNINumber { get; set; } = string.Empty;

    public int ApprenticeshipCourseLevel { get; set; }

    public DateTime ApprenticeshipCourseStartDate { get; set; }

    public bool IsHistoricProviderName { get; set; }
}