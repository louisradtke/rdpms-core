using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RDPMS.Core.Infra.Configuration;

/// <summary>
/// Settings for the application that may be changed during runtime. Logic that depends on this has to react to the
/// <see cref="PropertyChanged"/> event.
/// </summary>
public class RuntimeConfiguration : INotifyPropertyChanged
{
    private bool _testValue;

    public bool TestValue
    {
        get => _testValue;
        set => SetField(ref _testValue, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}