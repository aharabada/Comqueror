using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Comqueror.Utility;

public abstract class PropertyNotifier : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Invokes the <see cref="PropertyChanged"/>-event for the given property.
    /// </summary>
    /// <param name="name">The name of the property that changed. (Leave empty to use caller member name)</param>
    protected void NotifyPropertyChanged([CallerMemberName] string name = "")
    {
        PropertyChanged?.Invoke(this, new(name));
    }

    protected bool SetIfChanged<T>(ref T target, T newValue, [CallerMemberName] string name = "")
    {
        if ((target == null && newValue == null) || (target != null && target.Equals(newValue)))
            return false;

        target = newValue;

        NotifyPropertyChanged(name);

        return true;
    }
}
