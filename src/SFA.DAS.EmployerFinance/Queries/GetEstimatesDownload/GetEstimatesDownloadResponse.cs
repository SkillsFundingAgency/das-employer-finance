namespace SFA.DAS.EmployerFinance.Queries.GetEstimatesDownload;

public class GetEstimatesDownloadResponse
{
    public byte[] FileData { get; set; }
    public string FileExtension { get; set; }
    public string MimeType { get; set; }
}
