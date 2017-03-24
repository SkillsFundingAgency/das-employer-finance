﻿using MediatR;
using SFA.DAS.EAS.Application.Queries.GetTrainingProgrammes;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.ApprenticeshipCourse;
using SFA.DAS.EAS.Web.ViewModels;
using SFA.DAS.EAS.Web.ViewModels.ManageApprenticeships;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EAS.Web.Orchestrators.Mappers
{
    public class ApprenticeshipMapper : IApprenticeshipMapper
    {
        private readonly IHashingService _hashingService;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly IMediator _mediator;

        public ApprenticeshipMapper(
            IHashingService hashingService,
            ICurrentDateTime currentDateTime,
            IMediator mediator)
        {
            if (hashingService == null)
                throw new ArgumentNullException(nameof(hashingService));
            if (currentDateTime == null)
                throw new ArgumentNullException(nameof(currentDateTime));
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));

            _hashingService = hashingService;
            _currentDateTime = currentDateTime;
            _mediator = mediator;
        }

        public ApprenticeshipDetailsViewModel MapToApprenticeshipDetailsViewModel(Apprenticeship apprenticeship)
        {
            var statusText =
                apprenticeship.StartDate.HasValue
                && apprenticeship.StartDate.Value > new DateTime(_currentDateTime.Now.Year, _currentDateTime.Now.Month, 1)
                        ? "Waiting to start"
                        : MapPaymentStatus(apprenticeship.PaymentStatus);

            return new ApprenticeshipDetailsViewModel
            {
                HashedApprenticeshipId = _hashingService.HashValue(apprenticeship.Id),
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                DateOfBirth = apprenticeship.DateOfBirth,
                StartDate = apprenticeship.StartDate,
                EndDate = apprenticeship.EndDate,
                TrainingName = apprenticeship.TrainingName,
                Cost = apprenticeship.Cost,
                Status = statusText,
                ProviderName = string.Empty
            };
        }

        public ApprenticeshipViewModel MapToApprenticeshipViewModel(Apprenticeship apprenticeship)
        {
            return new ApprenticeshipViewModel
            {
                HashedApprenticeshipId = _hashingService.HashValue(apprenticeship.Id),
                HashedCommitmentId = _hashingService.HashValue(apprenticeship.CommitmentId),
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                NINumber = apprenticeship.NINumber,
                DateOfBirth = new DateTimeViewModel(apprenticeship.DateOfBirth?.Day, apprenticeship.DateOfBirth?.Month, apprenticeship.DateOfBirth?.Year),
                ULN = apprenticeship.ULN,
                TrainingType = apprenticeship.TrainingType,
                TrainingCode = apprenticeship.TrainingCode,
                TrainingName = apprenticeship.TrainingName,
                Cost = NullableDecimalToString(apprenticeship.Cost),
                StartDate = new DateTimeViewModel(apprenticeship.StartDate),
                EndDate = new DateTimeViewModel(apprenticeship.EndDate),
                PaymentStatus = apprenticeship.PaymentStatus,
                AgreementStatus = apprenticeship.AgreementStatus,
                ProviderRef = apprenticeship.ProviderRef,
                EmployerRef = apprenticeship.EmployerRef
            };
        }

        public async Task<Apprenticeship> MapFromAsync(ApprenticeshipViewModel viewModel)
        {
            var apprenticeship = new Apprenticeship
            {
                CommitmentId = _hashingService.DecodeValue(viewModel.HashedCommitmentId),
                Id = string.IsNullOrWhiteSpace(viewModel.HashedApprenticeshipId) ? 0L : _hashingService.DecodeValue(viewModel.HashedApprenticeshipId),
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                DateOfBirth = viewModel.DateOfBirth.DateTime,
                NINumber = viewModel.NINumber,
                ULN = viewModel.ULN,
                Cost = viewModel.Cost == null ? default(decimal?) : decimal.Parse(viewModel.Cost),
                StartDate = viewModel.StartDate.DateTime,
                EndDate = viewModel.EndDate.DateTime,
                ProviderRef = viewModel.ProviderRef,
                EmployerRef = viewModel.EmployerRef
            };

            if (!string.IsNullOrWhiteSpace(viewModel.TrainingCode))
            {
                var training = await GetTrainingProgramme(viewModel.TrainingCode);
                apprenticeship.TrainingType = training is Standard ? TrainingType.Standard : TrainingType.Framework;
                apprenticeship.TrainingCode = viewModel.TrainingCode;
                apprenticeship.TrainingName = training.Title;
            }

            return apprenticeship;
        }

        //public ApprenticeshipListItemViewModel MapToApprenticeshipListItem(Apprenticeship apprenticeship)
        //{
        //    return new ApprenticeshipListItemViewModel
        //    {
        //        HashedApprenticeshipId = _hashingService.HashValue(apprenticeship.Id),
        //        ApprenticeName = apprenticeship.ApprenticeshipName,
        //        ApprenticeDateOfBirth = apprenticeship.DateOfBirth,
        //        TrainingCode = apprenticeship.TrainingCode,
        //        TrainingName = apprenticeship.TrainingName,
        //        Cost = apprenticeship.Cost,
        //        StartDate = apprenticeship.StartDate,
        //        EndDate = apprenticeship.EndDate,
        //        CanBeApproved = apprenticeship.CanBeApproved
        //    };
        //}

        public async Task<UpdateApprenticeshipViewModel> CompareAndMapToApprenticeshipViewModel(
            Apprenticeship original, ApprenticeshipViewModel edited)
        {
            Func<string, string, string> changedOrNull = (a, edit) => 
                a?.Trim() == edit?.Trim() ? null : edit;

            // ToDo: The rest of the mapping
            var model = new UpdateApprenticeshipViewModel
            {
                // HashedApprenticeshipId = _hashingService.HashValue(apprenticeship.Id),
                // HashedCommitmentId = _hashingService.HashValue(apprenticeship.CommitmentId),
                FirstName = changedOrNull(original.FirstName, edited.FirstName),
                LastName = changedOrNull(original.LastName, edited.LastName),
                DateOfBirth = original.DateOfBirth == edited.DateOfBirth.DateTime
                    ? null
                    : edited.DateOfBirth,
                // ULN = apprenticeship.ULN,
                Cost = NullableDecimalToString(original.Cost) == edited.Cost 
                    ? default(decimal?) 
                    : string.IsNullOrEmpty(edited.Cost) ? 0m : decimal.Parse(edited.Cost),
                StartDate =  original.StartDate == edited.StartDate.DateTime
                  ? null
                  : edited.StartDate,
                EndDate = original.EndDate == edited.EndDate.DateTime 
                    ? null
                    : edited.EndDate,
                EmployerRef = changedOrNull(original.EmployerRef, edited.EmployerRef),
                OriginalApprenticeship = original
            };

            if (!string.IsNullOrWhiteSpace(edited.TrainingCode) && original.TrainingCode != edited.TrainingCode)
            {
                var training = await GetTrainingProgramme(edited.TrainingCode);
                model.TrainingType = training is Standard ? TrainingType.Standard : TrainingType.Framework;
                model.TrainingCode = edited.TrainingCode;
                model.TrainingName = training.Title;
            }

            return model;
        }

        private async Task<ITrainingProgramme> GetTrainingProgramme(string trainingCode)
        {
            // TODO: LWA - Need to check is this is called multiple times in a single request.
            var trainingProgrammes = await _mediator.SendAsync(new GetTrainingProgrammesQueryRequest());

            return trainingProgrammes.TrainingProgrammes.Where(x => x.Id == trainingCode).Single();
        }

        private static string NullableDecimalToString(decimal? item)
        {
            return (item.HasValue) ? ((int)item).ToString() : string.Empty;
        }

        private string MapPaymentStatus(PaymentStatus paymentStatus)
        {
            switch (paymentStatus)
            {
                case PaymentStatus.PendingApproval:
                    return "Approval needed";
                case PaymentStatus.Active:
                    return "On programme";
                case PaymentStatus.Paused:
                    return "Paused";
                case PaymentStatus.Withdrawn:
                    return "Stopped";
                case PaymentStatus.Completed:
                    return "Completed";
                case PaymentStatus.Deleted:
                    return "Deleted";
                default:
                    return string.Empty;
            }
        }
    }
}