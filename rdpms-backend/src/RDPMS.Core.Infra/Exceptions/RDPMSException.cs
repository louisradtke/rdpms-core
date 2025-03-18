using System.Runtime.Serialization;

namespace RDPMS.Core.Infra.Exceptions;

public class RDPMSException : Exception
{
    public RDPMSException()
    {
    }

    [Obsolete("Obsolete")]
    protected RDPMSException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public RDPMSException(string? message) : base(message)
    {
    }

    public RDPMSException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}