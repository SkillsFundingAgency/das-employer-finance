using SFA.DAS.EmployerFinance.Dtos;

namespace SFA.DAS.EmployerFinance.Web.ViewModels.Transfers
{
    public class TransferRequestsViewModel 
    {
        public long AccountId { get; set; }

        public IEnumerable<TransferRequestDto> TransferRequests { get; set; }

        public IEnumerable<TransferRequestDto> TransferSenderRequests =>
            TransferRequests.Where(p => p.SenderAccount.Id == AccountId);

        public IEnumerable<TransferRequestDto> TransferReceiverRequests =>
            TransferRequests.Where(p => p.ReceiverAccount.Id == AccountId);
    }
}