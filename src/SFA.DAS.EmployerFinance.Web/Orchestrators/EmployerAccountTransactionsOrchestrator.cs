using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Queries.FindAccountCoursePayments;
using SFA.DAS.EmployerFinance.Queries.FindAccountProviderPayments;
using SFA.DAS.EmployerFinance.Queries.FindEmployerAccountLevyDeclarationTransactions;
using SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;
using SFA.DAS.EmployerFinance.Queries.GetPayeSchemeByRef;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.Encoding;
using SFA.DAS.GovUK.Auth.Employer;
using AggregationData = SFA.DAS.EmployerFinance.Models.Transaction.AggregationData;
using ApprenticeshipEmployerType = SFA.DAS.Common.Domain.Types.ApprenticeshipEmployerType;
using TransactionItemType = SFA.DAS.EmployerFinance.Models.Transaction.TransactionItemType;
using TransactionViewModel = SFA.DAS.EmployerFinance.Web.ViewModels.TransactionViewModel;

namespace SFA.DAS.EmployerFinance.Web.Orchestrators;

public class EmployerAccountTransactionsOrchestrator(
    IAccountApiClient accountApiClient,
    IMediator mediator,
    ICurrentDateTime currentTime,
    ILogger<EmployerAccountTransactionsOrchestrator> logger,
    IEncodingService encodingService,
    IAuthenticationOrchestrator authenticationOrchestrator,
    IGovAuthEmployerAccountService accountService,
    EmployerFinanceWebConfiguration configuration)
    : IEmployerAccountTransactionsOrchestrator
{
    public virtual async Task<OrchestratorResponse<FinanceDashboardViewModel>> Index(string hashedAccountId, ClaimsIdentity userClaims)
    {
        //TODO this storing of user details should be removed from this applications database
        var email = userClaims.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var userId = userClaims.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var userAccountDetails = await accountService.GetUserAccounts(
            userId,
            email);
        if (!string.IsNullOrEmpty(userAccountDetails?.FirstName))
        {
            await authenticationOrchestrator.SaveIdentityAttributes(userAccountDetails.EmployerUserId, email, userAccountDetails.FirstName, userAccountDetails.LastName);    
        }
        
        var accountId = encodingService.Decode(hashedAccountId,EncodingType.AccountId);
        var accountDetailViewModel = await accountApiClient.GetAccount(accountId);
        
        logger.LogInformation("After GetAccount call");
        var getAccountFinanceOverview = await mediator.Send(new GetAccountFinanceOverviewQuery
        {
            AccountId = accountId
        });

        logger.LogInformation("account : {HashedAccountId}  getAccountFinanceOverview: {GetAccountFinanceOverview} ", hashedAccountId, getAccountFinanceOverview);
        logger.LogInformation(" account.ApprenticeshipEmployerType: {ApprenticeshipEmployerType}  HashedAccountId: {HashedAccountId} CurrentLevyFunds: {CurrentFunds} ", accountDetailViewModel.ApprenticeshipEmployerType, hashedAccountId, getAccountFinanceOverview.CurrentFunds);

        var viewModel = new OrchestratorResponse<FinanceDashboardViewModel>
        {
            Data = new FinanceDashboardViewModel
            {
                IsLevyEmployer = (ApprenticeshipEmployerType)Enum.Parse(typeof(ApprenticeshipEmployerType), accountDetailViewModel.ApprenticeshipEmployerType, true) == ApprenticeshipEmployerType.Levy,
                HashedAccountId = hashedAccountId,
                CurrentLevyFunds = getAccountFinanceOverview.CurrentFunds,
                TotalSpendForLastYear = getAccountFinanceOverview.TotalSpendForLastYear,
                FundingExpected = getAccountFinanceOverview.FundsIn,
                AvailableFunds = getAccountFinanceOverview.FundsIn - getAccountFinanceOverview.FundsOut,
                ShowLevyTransparency = configuration.ShowLevyTransparency
            }
        };

         return viewModel;
    }

    public async Task<OrchestratorResponse<PaymentTransactionViewModel>> FindAccountPaymentTransactions(
        string hashedId, long ukprn, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var data = await mediator.Send(new FindAccountProviderPaymentsQuery
            {
                HashedAccountId = hashedId,
                UkPrn = ukprn,
                FromDate = fromDate,
                ToDate = toDate
            });

            if (data == null)
            {
                return new OrchestratorResponse<PaymentTransactionViewModel>
                {
                    Status = HttpStatusCode.NotFound,
                    Exception = new Exception("Not found")
                };
            }
                
            var courseGroups = data.Transactions.GroupBy(x => new { x.CourseName, x.CourseLevel, x.CourseStartDate });

            var coursePaymentGroups = courseGroups.Select(x => new ApprenticeshipPaymentGroup
            {
                ApprenticeCourseName = x.Key.CourseName,
                CourseLevel = x.Key.CourseLevel,
                CourseStartDate = x.Key.CourseStartDate,
                Payments = x.ToList()
            }).ToList();


            return new OrchestratorResponse<PaymentTransactionViewModel>
            {
                Status = HttpStatusCode.OK,
                Data = new PaymentTransactionViewModel
                {
                    HashedAccountId = hashedId,
                    ProviderName = data.ProviderName,
                    TransactionDate = data.TransactionDate,
                    Amount = data.Total,
                    SubTransactions = data.Transactions,
                    CoursePaymentGroups = coursePaymentGroups
                }
            };
        }
        catch (ValidationException e)
        {
            return new OrchestratorResponse<PaymentTransactionViewModel>
            {
                Status = HttpStatusCode.BadRequest,
                Exception = e
            };
        }
        catch (UnauthorizedAccessException e)
        {
            return new OrchestratorResponse<PaymentTransactionViewModel>
            {
                Status = HttpStatusCode.Forbidden,
                Exception = e
            };
        }
    }

    public async Task<OrchestratorResponse<ProviderPaymentsSummaryViewModel>> GetProviderPaymentSummary(
        string hashedAccountId, long ukprn, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var accountTask = accountApiClient.GetAccount(hashedAccountId);
                
            var getProviderPaymentsTask = mediator.Send(new FindAccountProviderPaymentsQuery
            {
                HashedAccountId = hashedAccountId,
                UkPrn = ukprn,
                FromDate = fromDate,
                ToDate = toDate
            });                
                
            await Task.WhenAll(accountTask, getProviderPaymentsTask).ConfigureAwait(false);

            if (accountTask.Result == null || getProviderPaymentsTask.Result == null)
            {
                return new OrchestratorResponse<ProviderPaymentsSummaryViewModel>
                {
                    Status = HttpStatusCode.NotFound,
                    Exception = new Exception("Not found")
                };
            }
                
            var providerPaymentsResponse = getProviderPaymentsTask.Result;

            var courseGroups = providerPaymentsResponse.Transactions.GroupBy(x => new { x.CourseName, x.CourseLevel, x.PathwayName, x.CourseStartDate });

            var coursePaymentSummaries = courseGroups.Select(x =>
            {
                var levyPayments = x.Where(p => p.TransactionType == TransactionItemType.Payment).ToList();

                return new CoursePaymentSummaryViewModel
                {
                    CourseName = x.Key.CourseName,
                    CourseLevel = x.Key.CourseLevel,
                    PathwayName = x.Key.PathwayName,
                    PathwayCode = levyPayments.Max(p => p.PathwayCode),
                    CourseStartDate = x.Key.CourseStartDate,
                    LevyPaymentAmount = levyPayments.Sum(p => p.LineAmount),
                    EmployerCoInvestmentAmount = levyPayments.Sum(p => p.EmployerCoInvestmentAmount),
                    SFACoInvestmentAmount = levyPayments.Sum(p => p.SfaCoInvestmentAmount)
                };
            }).ToList();

            var accountResponse = accountTask.Result;

            return new OrchestratorResponse<ProviderPaymentsSummaryViewModel>
            {
                Status = HttpStatusCode.OK,
                Data = new ProviderPaymentsSummaryViewModel
                {
                    ApprenticeshipEmployerType = (ApprenticeshipEmployerType)Enum.Parse(typeof(ApprenticeshipEmployerType), accountResponse.ApprenticeshipEmployerType, true),
                    HashedAccountId = hashedAccountId,
                    UkPrn = ukprn,
                    ProviderName = providerPaymentsResponse.ProviderName,
                    PaymentDate = providerPaymentsResponse.DateCreated,
                    FromDate = fromDate,
                    ToDate = toDate,
                    CoursePayments = coursePaymentSummaries,
                    LevyPaymentsTotal = coursePaymentSummaries.Sum(p => p.LevyPaymentAmount),
                    SFACoInvestmentsTotal = coursePaymentSummaries.Sum(p => p.SFACoInvestmentAmount),
                    EmployerCoInvestmentsTotal = coursePaymentSummaries.Sum(p => p.EmployerCoInvestmentAmount),
                    PaymentsTotal = coursePaymentSummaries.Sum(p => p.TotalAmount)
                }
            };
        }
        catch (ValidationException e)
        {
            return new OrchestratorResponse<ProviderPaymentsSummaryViewModel>
            {
                Status = HttpStatusCode.BadRequest,
                Exception = e
            };
        }
        catch (UnauthorizedAccessException e)
        {
            return new OrchestratorResponse<ProviderPaymentsSummaryViewModel>
            {
                Status = HttpStatusCode.Forbidden,
                Exception = e
            };
        }
    }

    public virtual async Task<OrchestratorResponse<CoursePaymentDetailsViewModel>> GetCoursePaymentSummary(
        string hashedAccountId, long ukprn, string courseName, int? courseLevel, int? pathwayCode,
        DateTime fromDate, DateTime toDate)
    {
        try
        {
            var accountCoursePaymentsResponse = await mediator.Send(new FindAccountCoursePaymentsQuery
            {
                HashedAccountId = hashedAccountId,
                UkPrn = ukprn,
                CourseName = courseName,
                CourseLevel = courseLevel,
                PathwayCode = pathwayCode,
                FromDate = fromDate,
                ToDate = toDate
            });

            if (accountCoursePaymentsResponse == null)
            {
                return new OrchestratorResponse<CoursePaymentDetailsViewModel>
                {
                    Status = HttpStatusCode.NotFound,
                    Exception = new Exception("Not found")
                };
            }
                
            var apprenticePaymentGroups = accountCoursePaymentsResponse.Transactions.GroupBy(x => new { x.ApprenticeULN });

            var paymentSummaries = apprenticePaymentGroups.Select(pg =>
            {
                var payments = pg.Where(x => x.TransactionType == TransactionItemType.Payment).ToList();

                return new AprrenticeshipPaymentSummaryViewModel
                {
                    ApprenticeName = pg.First().ApprenticeName,
                    LevyPaymentAmount = payments.Sum(t => t.LineAmount),
                    SFACoInvestmentAmount = payments.Sum(p => p.SfaCoInvestmentAmount),
                    EmployerCoInvestmentAmount = payments.Sum(p => p.EmployerCoInvestmentAmount)
                };
            });

            var apprenticePayments = paymentSummaries.ToList();

            var accountResponse = await accountApiClient.GetAccount(hashedAccountId);

            return new OrchestratorResponse<CoursePaymentDetailsViewModel>
            {
                Status = HttpStatusCode.OK,
                Data = new CoursePaymentDetailsViewModel
                {
                    ApprenticeshipEmployerType = (ApprenticeshipEmployerType)Enum.Parse(typeof(ApprenticeshipEmployerType), accountResponse.ApprenticeshipEmployerType, true),
                    ProviderName = accountCoursePaymentsResponse.ProviderName,
                    CourseName = accountCoursePaymentsResponse.CourseName,
                    CourseLevel = accountCoursePaymentsResponse.CourseLevel,
                    PathwayName = accountCoursePaymentsResponse.PathwayName,
                    PaymentDate = accountCoursePaymentsResponse.DateCreated,
                    LevyPaymentsTotal = apprenticePayments.Sum(p => p.LevyPaymentAmount),
                    SFACoInvestmentTotal = apprenticePayments.Sum(p => p.SFACoInvestmentAmount),
                    EmployerCoInvestmentTotal = apprenticePayments.Sum(p => p.EmployerCoInvestmentAmount),
                    ApprenticePayments = apprenticePayments,
                    HashedAccountId = hashedAccountId
                }
            };
        }
        catch (ValidationException e)
        {
            return new OrchestratorResponse<CoursePaymentDetailsViewModel>
            {
                Status = HttpStatusCode.BadRequest,
                Exception = e
            };
        }
        catch (UnauthorizedAccessException e)
        {
            return new OrchestratorResponse<CoursePaymentDetailsViewModel>
            {
                Status = HttpStatusCode.Forbidden,
                Exception = e
            };
        }
    }

    public virtual async Task<OrchestratorResponse<TransactionViewResultViewModel>> GetAccountTransactions(
        string hashedId, int year, int month)
    {
        var employerAccountResult = await accountApiClient.GetAccount(hashedId);

        if (employerAccountResult == null)
        {
            return new OrchestratorResponse<TransactionViewResultViewModel>
            {
                Data = new TransactionViewResultViewModel(currentTime.Now)
            };
        }

        year = year == default ? DateTime.Now.Year : year;
        month = month == default ? DateTime.Now.Month : month;

        var aggregratedTransactions = await
                mediator.Send(new GetEmployerAccountTransactionsQuery
                {
                    FromDate = new DateTime(year, month, 1),
                    ToDate = new DateTime(year, month, DateTime.DaysInMonth(year, month)),
                    HashedAccountId = hashedId
                });

        var viewModel = BuildTransactionViewModel(aggregratedTransactions.Data);

        return new OrchestratorResponse<TransactionViewResultViewModel>
        {
            Data = new TransactionViewResultViewModel(currentTime.Now)
            {
                Account = employerAccountResult,
                Model = viewModel,
                Month = aggregratedTransactions.Month,
                Year = aggregratedTransactions.Year,
                AccountHasPreviousTransactions = aggregratedTransactions.AccountHasPreviousTransactions
            }
        };
    }

    private static TransactionViewModel BuildTransactionViewModel(AggregationData aggregationData)
    {
        var viewModel = new TransactionViewModel
        {
            Data = new AggregationData
            {
                AccountId = aggregationData.AccountId,
                HashedAccountId = aggregationData.HashedAccountId,
            },
            CurrentBalance = aggregationData.Balance
        };

        SetTransactionLines(viewModel, aggregationData);
        return viewModel;
    }

    private static void SetTransactionLines(TransactionViewModel viewModel, AggregationData aggregatedTransactionData)
    {
        var aggregatedLevyTransactions = aggregatedTransactionData.TransactionLines
            .Where(t => t.TransactionType == TransactionItemType.Declaration)
            .GroupBy(t => t.DateCreated.Date)
            .Select(grp =>
            {
                var firstLevyTransactionInDay = grp.First();
                return new TransactionLine
                {
                    AccountId = firstLevyTransactionInDay.AccountId,
                    DateCreated = firstLevyTransactionInDay.DateCreated,
                    Amount = grp.Sum(ltl => ltl.Amount),
                    TransactionType = TransactionItemType.Declaration,
                    Description = firstLevyTransactionInDay.Description,
                    PayrollDate = firstLevyTransactionInDay.PayrollDate,
                    PayrollMonth = firstLevyTransactionInDay.PayrollMonth,
                    PayrollYear = firstLevyTransactionInDay.PayrollYear
                };
            });

        var newTransactionLines = aggregatedTransactionData.TransactionLines
            .Where(t => t.TransactionType != TransactionItemType.Declaration)
            .Union(aggregatedLevyTransactions)
            .ToArray();

        viewModel.Data.TransactionLines = newTransactionLines;
    }

    public async Task<OrchestratorResponse<TransactionLineViewModel<LevyDeclarationTransactionLine>>> FindAccountLevyDeclarationTransactions(string hashedId, DateTime fromDate, DateTime toDate)
    {
        var data = await mediator.Send(new FindEmployerAccountLevyDeclarationTransactionsQuery
        {
            HashedAccountId = hashedId,
            FromDate = fromDate,
            ToDate = toDate
        });

        foreach (var transaction in data.Transactions)
        {
            var payeSchemeData = await mediator.Send(new GetPayeSchemeByRefQuery
            {
                HashedAccountId = hashedId,
                Ref = transaction.EmpRef
            });

            transaction.PayeSchemeName = payeSchemeData?.PayeScheme?.Name ?? string.Empty;
        }

        if (data.Transactions.Count == 0)
        {
            return new OrchestratorResponse<TransactionLineViewModel<LevyDeclarationTransactionLine>>
            {
                Status = HttpStatusCode.NotFound
            };
        }
        return new OrchestratorResponse<TransactionLineViewModel<LevyDeclarationTransactionLine>>
        {
            Status = HttpStatusCode.OK,
            Data = new TransactionLineViewModel<LevyDeclarationTransactionLine>
            {
                HashedAccountId = hashedId,
                Amount = data.Total,
                SubTransactions = data.Transactions,
                TransactionDate = data.Transactions.First().DateCreated
            }
        };
    }
}