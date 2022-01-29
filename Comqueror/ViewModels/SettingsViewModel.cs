using Comqueror.Properties;

namespace Comqueror.ViewModels;

public class SettingsViewModel : PropertyNotifier
{
    private bool _useStylisticSet;

    private RelayCommand? _saveSettings;

    public bool UseStylisticSet
    {
        get => _useStylisticSet;
        set => SetIfChanged(ref _useStylisticSet, value);
    }

    public RelayCommand SaveSettings => _saveSettings ??= new(o => Save());

    public SettingsViewModel()
    {
        _useStylisticSet = Settings.Default.Log_UseStylisticSet;
    }

    private void Save()
    {
        Settings.Default.Log_UseStylisticSet = _useStylisticSet;

        Settings.Default.Save();
    }
}
