using System.Runtime.Serialization;

namespace SFA.DAS.EmployerFinance.Http;

[Serializable]
public class ServiceUnavailableException : HttpException
{
    public ServiceUnavailableException() : base(503, "Service is unavailable") { }

    protected  ServiceUnavailableException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}