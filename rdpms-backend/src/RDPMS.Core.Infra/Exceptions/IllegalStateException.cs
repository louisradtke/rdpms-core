using System.Runtime.Serialization;

namespace RDPMS.Core.Infra.Exceptions;

public class IllegalStateException : RDPMSException
{
    public IllegalStateException()
    {
    }

    [Obsolete("Obsolete")]
    protected IllegalStateException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public IllegalStateException(string? message) : base(message)
    {
    }

    public IllegalStateException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}