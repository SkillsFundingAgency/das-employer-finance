using SFA.DAS.EmployerFinance.Commands.CreateEnglishFractionCalculationDate;
using SFA.DAS.EmployerFinance.Commands.RefreshEmployerLevyData;
using SFA.DAS.EmployerFinance.Commands.UpdateEnglishFractions;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.EmployerFinance.Models.HmrcLevy;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionsUpdateRequired;
using SFA.DAS.EmployerFinance.Queries.GetHMRCLevyDeclaration;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.MessageHandlers.CommandHandlers;

public class ImportAccountLevyDeclarationsCommandHandler : IHandleMessages<ImportAccountLevyDeclarationsCommand>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ImportAccountLevyDeclarationsCommandHandler> _logger;
    private readonly IDasAccountService _dasAccountService;
    private readonly string _declarationsEnabledValue;

    private bool HmrcProcessingEnabled => _declarationsEnabledValue.Equals("both", StringComparison.CurrentCultureIgnoreCase);

    private bool DeclarationProcessingOnly => _declarationsEnabledValue.Equals("declarations", StringComparison.CurrentCultureIgnoreCase);

    private bool FractionProcessingOnly => _declarationsEnabledValue.Equals("fractions", StringComparison.CurrentCultureIgnoreCase);

    public ImportAccountLevyDeclarationsCommandHandler(IMediator mediator, ILogger<ImportAccountLevyDeclarationsCommandHandler> logger, IDasAccountService dasAccountService, IConfiguration configuration)
    {
        _mediator = mediator;
        _logger = logger;
        _dasAccountService = dasAccountService;
        _declarationsEnabledValue = configuration.GetValue<string>("DeclarationsEnabled");
    }

    public async Task Handle(ImportAccountLevyDeclarationsCommand message, IMessageHandlerContext context)
    {
        try
        {
            var employerAccountId = message.AccountId;
            var payeRef = message.PayeRef;

            _logger.LogInformation($"Getting english fraction updates for employer account {employerAccountId}");

            var englishFractionUpdateResponse = await _mediator.Send(new GetEnglishFractionUpdateRequiredRequest());

            _logger.LogInformation($"Getting levy declarations for PAYE scheme {payeRef} for employer account {employerAccountId}");

            var payeSchemeDeclarations = await ProcessScheme(payeRef, englishFractionUpdateResponse);

            _logger.LogInformation($"Adding Levy Declarations of PAYE scheme {payeRef} to employer account {employerAccountId}");

            await RefreshEmployerAccountLevyDeclarations(employerAccountId, payeSchemeDeclarations);

            _logger.LogInformation($"{nameof(ImportAccountLevyDeclarationsCommand)} completed PAYE scheme: {payeRef}, employer account: {employerAccountId}");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"An error occurred importing levy for accountid='{message.AccountId}'");
            throw;
        }
    }

    private async Task RefreshEmployerAccountLevyDeclarations(long employerAccountId, ICollection<EmployerLevyData> payeSchemeDeclarations)
    {
        await _mediator.Send(new RefreshEmployerLevyDataCommand
        {
            AccountId = employerAccountId,
            EmployerLevyData = payeSchemeDeclarations
        });
    }

    private async Task<ICollection<EmployerLevyData>> ProcessScheme(string payeRef, GetEnglishFractionUpdateRequiredResponse englishFractionUpdateResponse)
    {
        var payeSchemeDeclarations = new List<EmployerLevyData>();

        await UpdateEnglishFraction(payeRef, englishFractionUpdateResponse);

        _logger.LogDebug($"Getting levy declarations from HMRC for PAYE scheme {payeRef}");

        var levyDeclarationQueryResult = HmrcProcessingEnabled || DeclarationProcessingOnly ?
            await _mediator.Send(new GetHMRCLevyDeclarationQuery { EmpRef = payeRef }) : null;

        _logger.LogDebug($"Processing levy declarations retrieved from HMRC for PAYE scheme {payeRef}");

        if (levyDeclarationQueryResult?.LevyDeclarations?.Declarations != null)
        {
            var declarations = CreateDasDeclarations(levyDeclarationQueryResult);

            var employerData = new EmployerLevyData
            {
                EmpRef = payeRef,
                Declarations = { Declarations = declarations }
            };

            payeSchemeDeclarations.Add(employerData);
        }

        return payeSchemeDeclarations;
    }

    private List<DasDeclaration> CreateDasDeclarations(GetHMRCLevyDeclarationResponse levyDeclarationQueryResult)
    {
        var declarations = new List<DasDeclaration>();

        foreach (var declaration in levyDeclarationQueryResult.LevyDeclarations.Declarations)
        {
            _logger.LogDebug($"Creating Levy Declaration with submission Id {declaration.SubmissionId} from HMRC query results");

            var dasDeclaration = new DasDeclaration
            {
                SubmissionDate = declaration.SubmissionTime,
                Id = declaration.Id,
                PayrollMonth = declaration.PayrollPeriod?.Month,
                PayrollYear = declaration.PayrollPeriod?.Year,
                LevyAllowanceForFullYear = declaration.LevyAllowanceForFullYear,
                LevyDueYtd = declaration.LevyDueYearToDate,
                NoPaymentForPeriod = declaration.NoPaymentForPeriod,
                DateCeased = declaration.DateCeased,
                InactiveFrom = declaration.InactiveFrom,
                InactiveTo = declaration.InactiveTo,
                SubmissionId = declaration.SubmissionId
            };

            declarations.Add(dasDeclaration);
        }

        return declarations;
    }

    private async Task UpdateEnglishFraction(string payeRef, GetEnglishFractionUpdateRequiredResponse englishFractionUpdateResponse)
    {
        if (HmrcProcessingEnabled || FractionProcessingOnly)
        {
            _logger.LogDebug($"Getting update for english fraction for PAYE scheme {payeRef}");
            await _mediator.Send(new UpdateEnglishFractionsCommand
            {
                EmployerReference = payeRef,
                EnglishFractionUpdateResponse = englishFractionUpdateResponse
            });

            _logger.LogDebug($"Updating english fraction for PAYE scheme {payeRef}");
            await _dasAccountService.UpdatePayeScheme(payeRef);
        }

        if (englishFractionUpdateResponse.UpdateRequired)
        {
            _logger.LogDebug($"Updating english fraction calculation date to " +
                          $"{englishFractionUpdateResponse.DateCalculated.ToShortDateString()} for PAYE scheme {payeRef}");

            await _mediator.Send(new CreateEnglishFractionCalculationDateCommand
            {
                DateCalculated = englishFractionUpdateResponse.DateCalculated
            });
        }
    }
}