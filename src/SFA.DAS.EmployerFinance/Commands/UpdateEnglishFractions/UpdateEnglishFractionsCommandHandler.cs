﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.Hmrc;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerFinance.Commands.UpdateEnglishFractions
{
    public class UpdateEnglishFractionsCommandHandler : IRequestHandler<UpdateEnglishFractionsCommand, Unit>
    {
        private readonly IHmrcService _hmrcService;
        private readonly IEnglishFractionRepository _englishFractionRepository;
        private readonly ILog _logger;

        public UpdateEnglishFractionsCommandHandler(IHmrcService hmrcService,
            IEnglishFractionRepository englishFractionRepository,
            ILog logger)
        {
            _hmrcService = hmrcService;
            _englishFractionRepository = englishFractionRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(UpdateEnglishFractionsCommand request,CancellationToken cancellationToken)
        {
            var existingFractions = (await _englishFractionRepository.GetAllEmployerFractions(request.EmployerReference)).ToList();

            if (existingFractions.Any() && !request.EnglishFractionUpdateResponse.UpdateRequired && TheFractionIsOlderOrEqualToTheUpdateDate(request, existingFractions))
            {
                //return Unit.Value;   
            }

            DateTime? dateFrom = null;
            if (existingFractions.OrderByDescending(x => x.DateCalculated).FirstOrDefault()?.DateCalculated != null 
                && existingFractions.OrderByDescending(x => x.DateCalculated).FirstOrDefault()?.DateCalculated != DateTime.MinValue)
            {
                dateFrom = existingFractions.OrderByDescending(x => x.DateCalculated).FirstOrDefault()?.DateCalculated.AddDays(-1);
            }


            var fractionCalculations = await _hmrcService.GetEnglishFractions(request.EmployerReference, dateFrom);

            if (fractionCalculations?.FractionCalculations == null)
            {
                return Unit.Value;
            }

            var hmrcFractions = fractionCalculations.FractionCalculations.SelectMany(calculations =>
            {
                var fractions = new List<DasEnglishFraction>();

                foreach (var fraction in calculations.Fractions)
                {
                    if (decimal.TryParse(fraction.Value, out _))
                    {
                        fractions.Add(
                            new DasEnglishFraction
                            {
                                EmpRef = fractionCalculations.Empref,
                                DateCalculated = calculations.CalculatedAt,
                                Amount = decimal.Parse(fraction.Value)
                            });
                    }
                    else
                    {
                        var exception = new ArgumentException($"Could not convert HMRC API fraction value {fraction.Value} to a decimal for english fraction update for EmpRef {request.EmployerReference}", nameof(fraction.Value));
                        _logger.Error(exception, exception.Message);
                    }
                }
             
                return fractions;
            }).ToList();

            var newFraction = hmrcFractions.Except(existingFractions, new DasEmployerComparer()).ToList();

            foreach (var englishFraction in newFraction)
            {
                await _englishFractionRepository.CreateEmployerFraction(englishFraction, englishFraction.EmpRef);
            }

            return Unit.Value;
        }

        private static bool TheFractionIsOlderOrEqualToTheUpdateDate(UpdateEnglishFractionsCommand message, List<DasEnglishFraction> existingFractions)
        {
            return message.EnglishFractionUpdateResponse.DateCalculated <=
                   existingFractions.OrderByDescending(x => x.DateCalculated).First().DateCalculated;
        }
    }

    public class DasEmployerComparer : IEqualityComparer<DasEnglishFraction>
    {
        public bool Equals(DasEnglishFraction source, DasEnglishFraction target)
        {
            return source.EmpRef.Equals(target?.EmpRef) &&
                   source.DateCalculated.Equals(target?.DateCalculated);
        }

        public int GetHashCode(DasEnglishFraction obj)
        {
            return obj.DateCalculated.GetHashCode() ^ obj.EmpRef.GetHashCode();
        }
    }
}
