namespace SFA.DAS.EmployerFinance.Models.ApprenticeshipCourse;

public class Standard : ITrainingProgramme
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Title { get; set; }
    public string CourseName { get; set; }
    public int Level { get; set; }
    public string LearningType { get; set; }
    public int Duration { get; set; }
    public int MaxFunding { get; set; }
}