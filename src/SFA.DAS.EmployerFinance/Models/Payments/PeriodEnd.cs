namespace SFA.DAS.EmployerFinance.Models.Payments;

public class PeriodEnd : Entity
{
    public virtual int Id { get; set; }

    public virtual string PeriodEndId { get; set; }

    public virtual int CalendarPeriodMonth { get; set; }

    public virtual int CalendarPeriodYear { get; set; }

    public virtual DateTime? AccountDataValidAt { get; set; }

    public virtual DateTime? CommitmentDataValidAt { get; set; }

    public virtual DateTime CompletionDateTime { get; set; }

    public virtual string PaymentsForPeriod { get; set; }
}