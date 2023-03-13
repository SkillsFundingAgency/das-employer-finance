using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferRequests;

public class GetTransferRequestsQueryHandler : IRequestHandler<GetTransferRequestsQuery, GetTransferRequestsResponse>
{
    private readonly IEmployerAccountRepository _employerAccountsRepository;
    private readonly IMapper _mapper;
    private readonly ICommitmentsV2ApiClient _commitmentV2ApiClient;
    private readonly IEncodingService _encodingService;        

    public GetTransferRequestsQueryHandler(
        IEmployerAccountRepository employerAccountsRepository,
        IMapper mapper,
        ICommitmentsV2ApiClient commitmentsV2Apiclient,
        IEncodingService encodingService)
    {
        _employerAccountsRepository = employerAccountsRepository;
        _mapper = mapper;
        _commitmentV2ApiClient = commitmentsV2Apiclient;
        _encodingService = encodingService;
    }

    public async Task<GetTransferRequestsResponse> Handle(GetTransferRequestsQuery message,CancellationToken cancellationToken)
    {
        var transferRequests = await _commitmentV2ApiClient.GetTransferRequests(message.AccountId);

        var accountIds = transferRequests.TransferRequestSummaryResponse
            .SelectMany(r => new[] { r.HashedSendingEmployerAccountId, r.HashedReceivingEmployerAccountId })
            .Select(h => _encodingService.Decode(h, EncodingType.AccountId))
            .ToList();

        var accountList = await _employerAccountsRepository.Get(accountIds);
        
        var accounts = accountList.Select(c=> new AccountDto
            {
                Id = c.Id,
                Name = c.Name,
                HashedId = _encodingService.Encode(c.Id, EncodingType.AccountId),
                PublicHashedId = _encodingService.Encode(c.Id, EncodingType.PublicAccountId)
            })
            .ToDictionary(p => p.HashedId);
            
        var transferRequestsData = transferRequests.TransferRequestSummaryResponse
            .Select(r => new TransferRequestDto
            {
                CreatedDate = r.CreatedOn,
                ReceiverAccount = accounts[r.HashedReceivingEmployerAccountId],
                SenderAccount = accounts[r.HashedSendingEmployerAccountId],
                Status = r.Status,
                TransferCost = r.TransferCost,
                TransferRequestHashedId = r.HashedTransferRequestId,
                Type = message.AccountId == accounts[r.HashedSendingEmployerAccountId].Id ? TransferConnectionType.Sender : TransferConnectionType.Receiver
            })
            .OrderBy(r => r.ReceiverAccount.Id == message.AccountId ? r.SenderAccount.Name : r.ReceiverAccount.Name)
            .ThenBy(r => r.CreatedDate)
            .ToList();

        return new GetTransferRequestsResponse
        {
            TransferRequests = transferRequestsData,
            AccountId = message.AccountId
        };
    }
}