using System.ComponentModel.DataAnnotations;
using AutoMapper.Configuration.Annotations;
using SFA.DAS.Authorization.ModelBinding;
using SFA.DAS.EmployerFinance.Attributes;
using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;
using SFA.DAS.EmployerFinance.Messages;

namespace SFA.DAS.EmployerFinance.Queries.GetTransactionsDownload;

public class GetTransactionsDownloadQuery : IAuthorizationContextModel, IRequest<GetTransactionsDownloadResponse>
{
    [Ignore]
    [Required]
    public long AccountId { get; set; }

    [Display(Name = "Start date")]
    [Required]
    [Month, Year, Date]
    public MonthYear StartDate { get; set; }

    [Display(Name = "End date")]
    [Required]
    [Month, Year, Date]
    public MonthYear EndDate { get; set; }

    [Required]
    public DownloadFormatType? DownloadFormat { get; set; }
}