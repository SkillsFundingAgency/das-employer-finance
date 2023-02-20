﻿namespace SFA.DAS.EmployerFinance.Http;

public class InternalServerErrorException : HttpException
{
    public InternalServerErrorException()
        : base(500, "Internal server error")
    {
    }
}