﻿using SFA.DAS.EmployerFinance.Web.ViewModels;

namespace SFA.DAS.EmployerFinance.Web.Orchestrators;

public class OrchestratorResponse
{
    public OrchestratorResponse()
    {
        this.Status = HttpStatusCode.OK;
    }
    public HttpStatusCode Status { get; set; }
    public Exception Exception { get; set; }
    public FlashMessageViewModel FlashMessage { get; set; }

    public string RedirectUrl { get; set; }
    public string RedirectButtonMessage { get; set; }
}
public class OrchestratorResponse<T> : OrchestratorResponse
{
    public T Data { get; set; }
}