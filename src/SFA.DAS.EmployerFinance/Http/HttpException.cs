using System.Runtime.Serialization;

namespace SFA.DAS.EmployerFinance.Http;

[Serializable]
public class HttpException : Exception
{
    public HttpException(int statusCode, string message, Exception innerException) : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public HttpException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }

    protected HttpException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public int StatusCode { get; }
}