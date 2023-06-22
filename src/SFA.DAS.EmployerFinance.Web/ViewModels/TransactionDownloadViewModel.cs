using SFA.DAS.EmployerFinance.Attributes;
using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;
using SFA.DAS.EmployerFinance.Messages;

namespace SFA.DAS.EmployerFinance.Web.ViewModels;

public class TransactionDownloadViewModel
{
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
    
    public static Dictionary<string, int> BuildPropertyOrderDictionary()
    {
        var propertyOrderDictionary = new Dictionary<string, int>
        {
            {"StartDate.Month", 0},
            {"StartDate.Year", 1},
            {"EndDate.Month", 2},
            {"EndDate.Year", 3}
        };

        return propertyOrderDictionary;
    }
}