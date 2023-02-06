﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetAccountBalances;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionCurrent;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFrationHistory;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclaration;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationsByAccountAndPeriod;
using SFA.DAS.EmployerFinance.Queries.GetTransferAllowance;
using SFA.DAS.HashingService;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators
{
    public class FinanceOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ILog _logger;
        private readonly IMapper _mapper;
        private readonly IHashingService _hashingService;

        public FinanceOrchestrator(
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

        public async Task<List<LevyDeclaration>> GetLevy(string hashedAccountId)
        {
            _logger.Info($"Requesting levy declaration for account {hashedAccountId}");

            var response = await _mediator.Send(new GetLevyDeclarationRequest { HashedAccountId = hashedAccountId });
            if (response.Declarations == null)
            {
                return null;
            }

            var levyDeclarations = response.Declarations.Select(x => _mapper.Map<LevyDeclaration>(x)).ToList();
            levyDeclarations.ForEach(x => x.HashedAccountId = hashedAccountId);
            _logger.Info($"Received response for levy declaration for account {hashedAccountId}");

            return levyDeclarations;
        }

        public async Task<List<LevyDeclaration>> GetLevy(string hashedAccountId, string payrollYear, short payrollMonth)
        {
            _logger.Info($"Requesting levy declaration for account {hashedAccountId}, year {payrollYear} and month {payrollMonth}");

            var response = await _mediator.Send(new GetLevyDeclarationsByAccountAndPeriodRequest { HashedAccountId = hashedAccountId, PayrollYear = payrollYear, PayrollMonth = payrollMonth });
            if (response.Declarations == null)
            {
                return null;
            }

            var levyDeclarations = response.Declarations.Select(x => _mapper.Map<LevyDeclaration>(x)).ToList();
            levyDeclarations.ForEach(x => x.HashedAccountId = hashedAccountId);
            _logger.Info($"Received response for levy declaration for account  {hashedAccountId}, year {payrollYear} and month {payrollMonth}");
            return levyDeclarations;
        }

        public async Task<List<DasEnglishFraction>> GetEnglishFractionHistory(string hashedAccountId, string empRef)
        {
            _logger.Info($"Requesting english fraction history for account {hashedAccountId}, empRef {empRef}");

            var response = await _mediator.Send(new GetEnglishFractionHistoryQuery { HashedAccountId = hashedAccountId, EmpRef = empRef });
            if (response.FractionDetail == null)
            {
                return null;
            }

            var dasEnglishFractions = response.FractionDetail.Select(x => _mapper.Map<DasEnglishFraction>(x)).ToList();
            _logger.Info($"Received response for english fraction history for account  {hashedAccountId}, empRef {empRef}");
            return dasEnglishFractions;
        }

        public async Task<List<DasEnglishFraction>> GetEnglishFractionCurrent(string hashedAccountId, string[] empRefs)
        {
            _logger.Info($"Requesting current english fractions for account {hashedAccountId}, empRefs {string.Join(", ", empRefs)}");

            var response = await _mediator.Send(new GetEnglishFractionCurrentQuery { HashedAccountId = hashedAccountId, EmpRefs = empRefs });
            if (response.Fractions == null)
            {
                return null;
            }

            var dasEnglishFractions = response.Fractions.Select(x => _mapper.Map<DasEnglishFraction>(x)).ToList();
            _logger.Info($"Received response for current english fractions for account  {hashedAccountId}, empRefs {string.Join(", ", empRefs)}");
            return dasEnglishFractions;
        }

        public async Task<List<AccountBalance>> GetAccountBalances(List<string> accountIds)
        {
            _logger.Info($"Requesting GetAccountBalances for the accounts");

            var decodedAccountIds = new List<long>();
            foreach (var id in accountIds)
            {
                try
                {
                    decodedAccountIds.Add(_hashingService.DecodeValue(id));
                }
                catch
                {
                    _logger.Info($"Exception thrown while decode hashedAccountId : { id}");
                }                
            }
            
            var response = await _mediator.Send(new GetAccountBalancesRequest
            {
                AccountIds = decodedAccountIds
            });

            var result = response?.Accounts.Select(x => _mapper.Map<AccountBalance>(x)).ToList();
            
            _logger.Info($"Received response - GetAccountBalances for the accounts { response?.Accounts.Count()}");
            
            return result;
        }

        public async Task<TransferAllowance> GetTransferAllowance(string hashedAccountId)
        {
            _logger.Info($"Requesting GetTransferAllowance for the hashedAccountId {hashedAccountId} ");

            var response = await _mediator.Send(new GetTransferAllowanceQuery
            {
                AccountId = _hashingService.DecodeValue(hashedAccountId)
            });

            var result = _mapper.Map<TransferAllowance>(response.TransferAllowance);

            _logger.Info($"Received response - GetTransferAllowance for the hashedAccountId {hashedAccountId} ");

            return result;
        }
    }
}