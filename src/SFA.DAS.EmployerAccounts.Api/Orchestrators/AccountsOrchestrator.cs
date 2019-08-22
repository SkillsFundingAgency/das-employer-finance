﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Queries.GetAccountTeamMembers;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccountDetail;
using SFA.DAS.EmployerAccounts.Queries.GetPagedEmployerAccounts;
using SFA.DAS.EmployerAccounts.Queries.GetPayeSchemeByRef;
using SFA.DAS.HashingService;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerAccounts.Api.Orchestrators
{
    public class AccountsOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ILog _logger;
        private readonly IMapper _mapper;
        private readonly IHashingService _hashingService;

        public AccountsOrchestrator(
            IMediator mediator, 
            ILog logger, 
            IMapper mapper,
            IHashingService hashingService)
        {
            _mediator = mediator;
            _logger = logger;
            _mapper = mapper;
            _hashingService = hashingService;
        }

        public async Task<PayeScheme> GetPayeScheme(string hashedAccountId, string payeSchemeRef)
        {
            _logger.Info($"Getting paye scheme {payeSchemeRef} for account {hashedAccountId}");

            var payeSchemeResult = await _mediator.SendAsync(new GetPayeSchemeByRefQuery { HashedAccountId = hashedAccountId, Ref = payeSchemeRef });
            return payeSchemeResult.PayeScheme == null ? null : ConvertToPayeScheme(hashedAccountId, payeSchemeResult);
        }

        public async Task<AccountDetail> GetAccount(string hashedAccountId)
        {
            _logger.Info($"Getting account {hashedAccountId}");

            var accountResult = await _mediator.SendAsync(new GetEmployerAccountDetailByHashedIdQuery { HashedAccountId = hashedAccountId });
            return accountResult.Account == null ? null : ConvertToAccountDetail(accountResult);
        }

        public async Task<PagedApiResponse<Account>> GetAccounts(string toDate, int pageSize, int pageNumber)
        {
            _logger.Info("Getting all accounts.");

            toDate = toDate ?? DateTime.MaxValue.ToString("yyyyMMddHHmmss");

            var accountsResult = await _mediator.SendAsync(new GetPagedEmployerAccountsQuery { ToDate = toDate, PageSize = pageSize, PageNumber = pageNumber });            

            var data = new List<Account>();          

            accountsResult.Accounts.ForEach(account =>
            {
                var accountModel = new Account
                {
                    AccountId = account.Id,
                    AccountName = account.Name,
                    AccountHashId = account.HashedId,
                    PublicAccountHashId = account.PublicHashedId,
                    IsLevyPayer = ((ApprenticeshipEmployerType) account.ApprenticeshipEmployerType) == ApprenticeshipEmployerType.Levy
                };

                data.Add(accountModel);
            });

            return new PagedApiResponse<Account>
            {
                Data = data,
                Page = pageNumber,
                TotalPages = (accountsResult.AccountsCount / pageSize) + 1
            };
        }

        public async Task<List<TeamMember>> GetAccountTeamMembers(string hashedAccountId)
        {
            _logger.Info($"Requesting team members for account {hashedAccountId}");

            var teamMembers = await _mediator.SendAsync(new GetAccountTeamMembersQuery { HashedAccountId = hashedAccountId });
            return teamMembers.TeamMembers.Select(x => _mapper.Map<TeamMember>(x)).ToList();
        }

        public async Task<List<TeamMember>> GetAccountTeamMembers(long accountId)
        {
            var hashedAccountId = _hashingService.HashValue(accountId);
            return await GetAccountTeamMembers(hashedAccountId);
        }

        private PayeScheme ConvertToPayeScheme(string hashedAccountId, GetPayeSchemeByRefResponse payeSchemeResult)
        {
            return new PayeScheme
            {
                DasAccountId = hashedAccountId,
                Name = payeSchemeResult.PayeScheme.Name,
                Ref = payeSchemeResult.PayeScheme.Ref,
                AddedDate = payeSchemeResult.PayeScheme.AddedDate,
                RemovedDate = payeSchemeResult.PayeScheme.RemovedDate
            };
        }

        private static AccountDetail ConvertToAccountDetail(GetEmployerAccountDetailByHashedIdResponse accountResult)
        {
            return new AccountDetail
            {
                AccountId = accountResult.Account.AccountId,
                HashedAccountId = accountResult.Account.HashedId,
                PublicHashedAccountId = accountResult.Account.PublicHashedId,
                DateRegistered = accountResult.Account.CreatedDate,
                OwnerEmail = accountResult.Account.OwnerEmail,
                DasAccountName = accountResult.Account.Name,
                LegalEntities = new ResourceList(accountResult.Account.LegalEntities.Select(x => new Resource { Id = x.ToString() })),
                PayeSchemes = new ResourceList(accountResult.Account.PayeSchemes.Select(x => new Resource { Id = x })),
                ApprenticeshipEmployerType = accountResult.Account.ApprenticeshipEmployerType.ToString(),
                AccountAgreementType = GetAgreementType(accountResult)
            };
        }

        private static AccountAgreementType GetAgreementType(GetEmployerAccountDetailByHashedIdResponse accountResult)
        {
            var agreementTypeGroup = accountResult.Account.AccountAgreementTypes?
                .GroupBy(x => x);

            if (agreementTypeGroup == null || !agreementTypeGroup.Any())
            {
                return AccountAgreementType.Unknown;
            }

            return agreementTypeGroup?.Count() > 1 ? AccountAgreementType.Inconsistent : (AccountAgreementType)Enum.Parse(typeof(AccountAgreementType), agreementTypeGroup?.FirstOrDefault()?.Key.ToString());
        }
    }
}