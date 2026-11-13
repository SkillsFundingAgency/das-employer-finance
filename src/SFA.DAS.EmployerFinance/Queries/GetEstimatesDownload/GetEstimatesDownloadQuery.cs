using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;

namespace SFA.DAS.EmployerFinance.Queries.GetEstimatesDownload;

public sealed record GetEstimatesDownloadQuery(long AccountId, DownloadFormatType DownloadFormat) : IRequest<GetEstimatesDownloadResponse>;
