using System.Runtime.Serialization;

namespace RDPMS.Core.Infra.Exceptions;

public class IllegalArgumentException : RDPMSException
{
    public IllegalArgumentException()
    {
    }

    [Obsolete("Obsolete")]
    protected IllegalArgumentException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public IllegalArgumentException(string? message) : base(message)
    {
    }

    public IllegalArgumentException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}