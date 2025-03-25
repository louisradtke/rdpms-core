namespace RDPMS.Core.Server.Util;

/// <summary>
/// Structure that can automatically check an item.
/// </summary>
/// <typeparam name="T">Type of the item to check for specific properties</typeparam>
public class ConstraintChecker<T>
{
    private List<CheckSet<T>> Checks { get; } = new();

    public ErrorCollection Check(T obj)
    {
        var errors = new ErrorCollection();

        foreach (var check in Checks)
        {
            if (check.IsConditional)
                continue;

            var result = check.CheckFunc(obj);
            if (result)
                continue;
            
            var message = check.MessageFunc(obj);
            errors.Add(new SimpleCollectableError(message, check.Severity));
        }

        return errors;
    }
    
    /// <summary>
    /// Check, that gets executed always.
    /// </summary>
    /// <param name="check">Condition that must evaluate true.</param>
    /// <param name="message">Function that evaluates message that shall be put, if condition fails.</param>
    /// <param name="severity">Error severity.</param>
    public void AddCheck(Func<T, bool> check, Func<T, string> message, ErrorSeverity severity)
    {
        Checks.Add(new CheckSet<T>()
        {
            CheckFunc = check,
            MessageFunc = message,
            Severity = severity,
            IsConditional = false
        });
    }

    /// <summary>
    /// Check, that gets executed always.
    /// Severity for this error will be set to <see cref="ErrorSeverity.Warning"/>.
    /// </summary>
    /// <param name="check">Condition that must evaluate true.</param>
    /// <param name="message">Function that evaluates message that shall be put, if condition fails.</param>
    public void AddCheckWarning(Func<T, bool> check, Func<T, string> message)
        => AddCheck(check, message, ErrorSeverity.Warning);

    /// <summary>
    /// Check, that gets executed always.
    /// Severity for this error will be set to <see cref="ErrorSeverity.Error"/>.
    /// </summary>
    /// <param name="check">Condition that must evaluate true.</param>
    /// <param name="message">Function that evaluates message that shall be put, if condition fails.</param>
    public void AddCheckError(Func<T, bool> check, Func<T, string> message)
        => AddCheck(check, message, ErrorSeverity.Error);

    /// <summary>
    /// Check, that gets only gets executed, if no error with severity of <see cref="ErrorSeverity.Error"/> exits.
    /// </summary>
    /// <param name="check">Condition that must evaluate true.</param>
    /// <param name="message">Function that evaluates message that shall be put, if condition fails.</param>
    /// <param name="severity">Error severity.</param>
    public void AddConditionalCheck(Func<T, bool> check, Func<T, string> message, ErrorSeverity severity)
    {
        Checks.Add(new CheckSet<T>()
        {
            CheckFunc = check,
            MessageFunc = message,
            Severity = severity,
            IsConditional = true
        });
    }

    /// <summary>
    /// Check, that gets only gets executed, if no error with severity of <see cref="ErrorSeverity.Error"/> exits.
    /// Severity for this error will be set to <see cref="ErrorSeverity.Warning"/>.
    /// </summary>
    /// <param name="check">Condition that must evaluate true.</param>
    /// <param name="message">Function that evaluates message that shall be put, if condition fails.</param>
    public void AddConditionalCheckWarning(Func<T, bool> check, Func<T, string> message)
        => AddConditionalCheck(check, message, ErrorSeverity.Warning);

    /// <summary>
    /// Check, that gets only gets executed, if no error with severity of <see cref="ErrorSeverity.Error"/> exits.
    /// Severity for this error will be set to <see cref="ErrorSeverity.Error"/>.
    /// </summary>
    /// <param name="check">Condition that must evaluate true.</param>
    /// <param name="message">Function that evaluates message that shall be put, if condition fails.</param>
    public void AddConditionalCheckError(Func<T, bool> check, Func<T, string> message)
        => AddConditionalCheck(check, message, ErrorSeverity.Error);
    
    public void AddCheckSet(CheckSet<T> checkSet) => Checks.Add(checkSet);
    
    public void AddCheckSets(IEnumerable<CheckSet<T>> checkSets) => Checks.AddRange(checkSets);
}