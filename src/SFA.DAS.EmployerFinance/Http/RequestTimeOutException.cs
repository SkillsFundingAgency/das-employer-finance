using System.Runtime.Serialization;

namespace SFA.DAS.EmployerFinance.Http;

[Serializable]
public class RequestTimeOutException : HttpException
{
    public RequestTimeOutException() : base(408, "Request has time out") { }

    protected RequestTimeOutException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}