using System;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MediatR;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Commands.RunHealthCheckCommand
{
    public class RunHealthCheckCommand : IAuthorizationContextModel, IRequest<Unit>
    {
        [IgnoreMap]
        [Required]
        public Guid? UserRef { get; set; }
    }
}