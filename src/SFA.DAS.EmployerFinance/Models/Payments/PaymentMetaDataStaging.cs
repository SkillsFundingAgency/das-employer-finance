namespace SFA.DAS.EmployerFinance.Models.Payments;

public class PaymentMetaDataStaging : PaymentMetaData
{
    public string CreatedBy { get; set; }

    public Guid? CorrelationId { get; set; }
}
