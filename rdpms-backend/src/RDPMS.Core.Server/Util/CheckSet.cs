namespace RDPMS.Core.Server.Util;

/// <summary>
/// Contains a check and its implications, if a check fails (evaluates false).
/// Implications are e.g., which message to put or which severity a failing check has.
/// </summary>
/// <typeparam name="T">Type of the object to check.</typeparam>
public class CheckSet<T>
{
    public required Func<T, bool> CheckFunc { get; init; }
    public required ErrorSeverity Severity { get; init; }
    public required Func<T, string> MessageFunc { get; init; }
    public required bool IsConditional { get; init; }

    /// <summary>
    /// Create instance with <see cref="Severity"/> and <see cref="IsConditional"/> already set accordingly.
    /// Check is non-conditional, so will always be executed.
    /// </summary>
    /// <param name="check">Check func (true is pass).</param>
    /// <param name="message">Message to pass on failed check.</param>
    /// <returns>A new instance</returns>
    public static CheckSet<T> CreateErr(Func<T, bool> check, Func<T, string> message)
    {
        return new CheckSet<T>()
        {
            CheckFunc = check,
            MessageFunc = message,
            Severity = ErrorSeverity.Error,
            IsConditional = false
        };
    }

    /// <summary>
    /// Create instance with <see cref="Severity"/> and <see cref="IsConditional"/> already set accordingly.
    /// Check is conditional, so will only be executed if no prior checks in <see cref="ConstraintChecker{T}"/>
    /// have severity <see cref="ErrorSeverity.Error"/> or higher.
    /// </summary>
    /// <param name="check">Check func (true is pass).</param>
    /// <param name="message">Message to pass on failed check.</param>
    /// <returns>A new instance</returns>
    public static CheckSet<T> CreateErrCond(Func<T, bool> check, Func<T, string> message)
    {
        return new CheckSet<T>()
        {
            CheckFunc = check,
            MessageFunc = message,
            Severity = ErrorSeverity.Error,
            IsConditional = true
        };
    }
    
    /// <summary>
    /// Create instance with <see cref="Severity"/> and <see cref="IsConditional"/> already set accordingly.
    /// Check is non-conditional, so will always be executed.
    /// </summary>
    /// <param name="check">Check func (true is pass).</param>
    /// <param name="message">Message to pass on failed check.</param>
    /// <returns>A new instance</returns>
    public static CheckSet<T> CreateWarn(Func<T, bool> check, Func<T, string> message)
    {
        return new CheckSet<T>()
        {
            CheckFunc = check,
            MessageFunc = message,
            Severity = ErrorSeverity.Warning,
            IsConditional = false
        };
    }

    /// <summary>
    /// Create instance with <see cref="Severity"/> and <see cref="IsConditional"/> already set accordingly.
    /// Check is conditional, so will only be executed if no prior checks in <see cref="ConstraintChecker{T}"/>
    /// have severity <see cref="ErrorSeverity.Error"/> or higher.
    /// </summary>
    /// <param name="check">Check func (true is pass).</param>
    /// <param name="message">Message to pass on failed check.</param>
    /// <returns>A new instance</returns>
    public static CheckSet<T> CreateWarnCond(Func<T, bool> check, Func<T, string> message)
    {
        return new CheckSet<T>()
        {
            CheckFunc = check,
            MessageFunc = message,
            Severity = ErrorSeverity.Warning,
            IsConditional = true
        };
    }    
}