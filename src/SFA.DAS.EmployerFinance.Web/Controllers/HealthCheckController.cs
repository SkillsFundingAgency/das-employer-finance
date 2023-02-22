﻿using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerFinance.Commands.RunHealthCheckCommand;
using SFA.DAS.EmployerFinance.Queries.GetHealthCheck;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.ViewModels;

namespace SFA.DAS.EmployerFinance.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [Route("healthcheck")]
    public class HealthCheckController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public HealthCheckController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(GetHealthCheckQuery query)
        {
            var response = await _mediator.Send(query);
            var model = _mapper.Map<HealthCheckViewModel>(response);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RunHealthCheckCommand command)
        {
            await _mediator.Send(command);

            return RedirectToAction("Index");
        }
    }
}