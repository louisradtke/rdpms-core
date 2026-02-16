using System.Runtime.Serialization;

namespace RDPMS.Core.Infra.Exceptions;

public class QueryException : RDPMSException
{
    public string? PublicMessage { get; init; }

    public QueryException()
    {
    }

    [Obsolete("Obsolete")]
    protected QueryException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public QueryException(string? message) : base(message)
    {
    }

    public QueryException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public QueryException(string? message, string publicMessage) : base(message)
    {
        PublicMessage = publicMessage;
    }

    public QueryException(string? message, Exception? innerException, string publicMessage) : base(message, innerException)
    {
        PublicMessage = publicMessage;
    }

}