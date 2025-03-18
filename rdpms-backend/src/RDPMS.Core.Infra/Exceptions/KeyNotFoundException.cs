using System.Runtime.Serialization;

namespace RDPMS.Core.Infra.Exceptions;

public class KeyNotFoundException : RDPMSException
{
    public KeyNotFoundException()
    {
    }

    [Obsolete("Obsolete")]
    protected KeyNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public KeyNotFoundException(string? message) : base(message)
    {
    }

    public KeyNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}