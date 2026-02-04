using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Dtos;

public class TransferStagingDto
{
    public long TransferId { get; set; }
    public long SenderAccountId { get; set; }
    public long ReceiverAccountId { get; set; }
    public string ReceiverAccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransferDate { get; set; }
    public string PeriodEnd { get; set; } = string.Empty;
    public short CollectionPeriodMonth { get; set; }
    public short CollectionPeriodYear { get; set; }
    public long Ukprn { get; set; }
    public string CourseName { get; set; } = string.Empty;
}
