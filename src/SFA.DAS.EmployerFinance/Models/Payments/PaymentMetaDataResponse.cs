namespace SFA.DAS.EmployerFinance.Models.Payments;

public class PaymentMetaDataResponse
{
    public long MetadataId { get; set; }

    public bool Upserted { get; set; }

    public bool IsSuccess { get; set; }

    public bool HasValidationErrors { get; set; }

    public List<string> ValidationErrors { get; set; } = [];
    public bool NotFound { get; set; }
}
