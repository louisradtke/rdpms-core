using System.Runtime.Serialization;

namespace RDPMS.Core.Infra.Exceptions;

public class UnknownArgumentException : RDPMSException
{
    public UnknownArgumentException()
    {
    }

    [Obsolete("Obsolete")]
    protected UnknownArgumentException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public UnknownArgumentException(string? message) : base(message)
    {
    }

    public UnknownArgumentException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}