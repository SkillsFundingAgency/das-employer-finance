using System.Runtime.Serialization;

namespace SFA.DAS.EmployerFinance.Http;

[Serializable]
public class TooManyRequestsException : HttpException
{
    public TooManyRequestsException() : base(429, "Rate limit has been reached") { }

    protected TooManyRequestsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}