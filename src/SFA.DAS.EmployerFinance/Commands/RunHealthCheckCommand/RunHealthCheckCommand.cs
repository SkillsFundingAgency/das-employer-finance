﻿using System.ComponentModel.DataAnnotations;
using AutoMapper.Configuration.Annotations;

namespace SFA.DAS.EmployerFinance.Commands.RunHealthCheckCommand;

public class RunHealthCheckCommand : IRequest
{
    [Ignore]
    [Required]
    public Guid? UserRef { get; set; }
}