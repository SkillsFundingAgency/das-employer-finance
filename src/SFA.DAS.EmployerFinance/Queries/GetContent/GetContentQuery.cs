﻿namespace SFA.DAS.EmployerFinance.Queries.GetContent;

public class GetContentQuery : IRequest<GetContentResponse>
{
    public string ContentType { get; set; }
    public bool UseLegacyStyles { get; set; }
}