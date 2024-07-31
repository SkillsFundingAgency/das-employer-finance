using System.ComponentModel.DataAnnotations;
using System.Text;
using SFA.DAS.EmployerFinance.Commands.PublishGenericEvent;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Factories;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerFinance.Commands.RefreshEmployerLevyData;

public class RefreshEmployerLevyDataCommandHandler : IRequestHandler<RefreshEmployerLevyDataCommand>
{
    private readonly IValidator<RefreshEmployerLevyDataCommand> _validator;
    private readonly IDasLevyRepository _dasLevyRepository;
    private readonly IMediator _mediator;
    private readonly ILevyEventFactory _levyEventFactory;
    private readonly IGenericEventFactory _genericEventFactory;
    private readonly IEncodingService _encodingService;
    private readonly ILevyImportCleanerStrategy _levyImportCleanerStrategy;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<RefreshEmployerLevyDataCommandHandler> _logger;

    public RefreshEmployerLevyDataCommandHandler(
        IValidator<RefreshEmployerLevyDataCommand> validator,
        IDasLevyRepository dasLevyRepository,
        IMediator mediator,
        ILevyEventFactory levyEventFactory,
        IGenericEventFactory genericEventFactory,
        IEncodingService encodingService,
        ILevyImportCleanerStrategy levyImportCleanerStrategy,
        IEventPublisher eventPublisher,
        ILogger<RefreshEmployerLevyDataCommandHandler> logger)
    {
        _validator = validator;
        _dasLevyRepository = dasLevyRepository;
        _mediator = mediator;
        _levyEventFactory = levyEventFactory;
        _genericEventFactory = genericEventFactory;
        _encodingService = encodingService;
        _levyImportCleanerStrategy = levyImportCleanerStrategy;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(RefreshEmployerLevyDataCommand request, CancellationToken cancellationToken)
    {
        var result = _validator.Validate(request);

        if (!result.IsValid())
        {
            throw new ValidationException(result.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var savedDeclarations = new List<DasDeclaration>();
        var updatedEmpRefs = new List<string>();

        foreach (var employerLevyData in request.EmployerLevyData)
        {
            var declarations = await _levyImportCleanerStrategy.Cleanup(employerLevyData.EmpRef, employerLevyData.Declarations.Declarations);

            if (declarations.Length == 0) continue;

            await _dasLevyRepository.CreateEmployerDeclarations(declarations, employerLevyData.EmpRef, request.AccountId);

            updatedEmpRefs.Add(employerLevyData.EmpRef);
            savedDeclarations.AddRange(declarations);
        }

        var hasDecalarations = savedDeclarations.Any();
        var levyTotalTransactionValue = decimal.Zero;

        if (hasDecalarations)
        {
            levyTotalTransactionValue = await HasAccountHadLevyTransactions(request, updatedEmpRefs);
            await PublishDeclarationUpdatedEvents(request.AccountId, savedDeclarations);
        }

        await PublishRefreshEmployerLevyDataCompletedEvent(hasDecalarations, levyTotalTransactionValue, request.AccountId);
        await PublishAccountLevyStatusEvent(levyTotalTransactionValue, request.AccountId);
    }

    private Task PublishRefreshEmployerLevyDataCompletedEvent(bool levyImported, decimal levyTotalTransactionValue, long accountId)
    {
        return _eventPublisher.Publish(new RefreshEmployerLevyDataCompletedEvent
        {
            AccountId = accountId,
            Created = DateTime.UtcNow,
            LevyImported = levyImported,
            LevyTransactionValue = levyTotalTransactionValue
        });
    }

    private async Task PublishAccountLevyStatusEvent(decimal levyTotalTransactionValue, long accountId)
    {
        if (levyTotalTransactionValue != decimal.Zero)
        {
            await _eventPublisher.Publish(new LevyAddedToAccount
            {
                AccountId = accountId,
                Amount = levyTotalTransactionValue
            });
        }
    }

    private async Task<decimal> HasAccountHadLevyTransactions(RefreshEmployerLevyDataCommand message, IEnumerable<string> updatedEmpRefs)
    {
        var levyTransactionTotalAmount = decimal.Zero;

        foreach (var empRef in updatedEmpRefs)
        {
            _logger.LogInformation($"Processing declarations for {message.AccountId}, PAYE: {ObscurePayeScheme(empRef)}");
            levyTransactionTotalAmount += await _dasLevyRepository.ProcessDeclarations(message.AccountId, empRef);
        }

        return levyTransactionTotalAmount;
    }

    private Task PublishDeclarationUpdatedEvents(long accountId, IEnumerable<DasDeclaration> savedDeclarations)
    {
        var hashedAccountId = _encodingService.Encode(accountId, EncodingType.AccountId);

        var periodsChanged = savedDeclarations.Select(x =>
            new
            {
                x.PayrollYear,
                x.PayrollMonth
            }).Distinct();

        var tasks = periodsChanged.Select(x => CreateDeclarationUpdatedEvent(hashedAccountId, x.PayrollYear, x.PayrollMonth));
        return Task.WhenAll(tasks);
    }

    private Task CreateDeclarationUpdatedEvent(string hashedAccountId, string payrollYear, short? payrollMonth)
    {
        var declarationUpdatedEvent = _levyEventFactory.CreateDeclarationUpdatedEvent(hashedAccountId, payrollYear, payrollMonth);
        var genericEvent = _genericEventFactory.Create(declarationUpdatedEvent);

        return _mediator.Send(new PublishGenericEventCommand { Event = genericEvent });
    }

    private static string ObscurePayeScheme(string payeSchemeId)
    {
        var length = payeSchemeId.Length;

        var response = new StringBuilder(payeSchemeId);

        for (var i = 1; i < length - 1; i++)
            if (response[i].ToString() != "/")
            {
                response.Remove(i, 1);
                response.Insert(i, "*");
            }

        return response.ToString();
    }
}