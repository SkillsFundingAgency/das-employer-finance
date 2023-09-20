using System.ComponentModel.DataAnnotations;
using AutoMapper;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.SendTransferConnectionInvitation;

public class SendTransferConnectionInvitationQueryHandler : IRequestHandler<SendTransferConnectionInvitationQuery, SendTransferConnectionInvitationResponse>
{
    private readonly IEmployerAccountRepository _employerAccountRepository;
    private readonly ITransferConnectionInvitationRepository _transferConnectionInvitationRepository;
    private readonly IMapper _mapper;
    private readonly IEncodingService _encodingService;

    public SendTransferConnectionInvitationQueryHandler(
        IEmployerAccountRepository employerAccountRepository,
        ITransferConnectionInvitationRepository transferConnectionInvitationRepository,
        IMapper mapper,
        IEncodingService encodingService)
    {
        _employerAccountRepository = employerAccountRepository;
        _transferConnectionInvitationRepository = transferConnectionInvitationRepository;
        _mapper = mapper;
        _encodingService = encodingService;
    }

    public async Task<SendTransferConnectionInvitationResponse> Handle(SendTransferConnectionInvitationQuery message,CancellationToken cancellationToken)
    {
        var result = _encodingService.TryDecode(message.ReceiverAccountPublicHashedId, EncodingType.PublicAccountId, out long decode);
        if (!result)
        {
            ThrowValidationException("ReceiverAccountPublicHashedId", "You must enter a valid account ID");
        }
        var receiverAccount = await _employerAccountRepository.Get(decode);

        if (receiverAccount == null || receiverAccount.Id == message.AccountId)
        {
            ThrowValidationException("ReceiverAccountPublicHashedId", "You must enter a valid account ID");
        }

        var anyTransferConnectionToSameReceiverInvitations = 
            await _transferConnectionInvitationRepository.AnyTransferConnectionInvitations(
                message.AccountId, 
                receiverAccount.Id, 
                new List<TransferConnectionInvitationStatus> { TransferConnectionInvitationStatus.Pending, TransferConnectionInvitationStatus.Approved});
               
        if (anyTransferConnectionToSameReceiverInvitations)
        {
            ThrowValidationException("ReceiverAccountPublicHashedId", "You can't connect with this employer because they already have a pending or accepted connection request");
        }

        var senderAccount = await _employerAccountRepository.Get(message.AccountId);
        
        return new SendTransferConnectionInvitationResponse
        {
            ReceiverAccount = _mapper.Map<AccountDto>(receiverAccount),
            SenderAccount = _mapper.Map<AccountDto>(senderAccount),
        };
    }

    private static void ThrowValidationException(string property, string message)
    {
        var validationResult = new Validation.ValidationResult();
        validationResult.AddError(property,message);
        var ex = new ValidationException();
        throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
    }
}