using Comqueror.Properties;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Comqueror.ViewModels;

public class LanguageItem
{
    private string _culture;

    public LanguageItem(string culture)
    {
        _culture = culture;
    }

    public string Culture => _culture;

    public string? LocalizedName => Strings.ResourceManager.GetString($"Language_{Culture}");
}

public class SettingsViewModel : PropertyNotifier
{
    private readonly List<LanguageItem> _languages = new()
    {
        new LanguageItem("en-US"),
        new LanguageItem("de-DE"),
    };

    public List<LanguageItem> Languages => _languages;

    private LanguageItem _language;

    private bool _settingsChanged;

    public LanguageItem Language
    {
        get => _language;
        set => SetIfChanged(ref _language, value);
    }

    private bool _useStylisticSet;

    private RelayCommand? _saveSettings;

    public bool UseStylisticSet
    {
        get => _useStylisticSet;
        set => SetIfChanged(ref _useStylisticSet, value);
    }

    public bool SettingsChanged
    {
        get => _settingsChanged;
        set => SetIfChanged(ref _settingsChanged, value);
    }

    public RelayCommand SaveSettings => _saveSettings ??= new(o => Save());

    protected new bool SetIfChanged<T>(ref T target, T newValue, [CallerMemberName] string name = "")
    {
        bool changed = base.SetIfChanged(ref target, newValue, name);

        if (changed)
            SettingsChanged = true;

        return changed;
    }

    public SettingsViewModel()
    {
        _useStylisticSet = Settings.Default.Log_UseStylisticSet;

        string? culture = Settings.Default.Culture;

        LanguageItem? language = _languages.FirstOrDefault(l => l.Culture == culture);

        if (language == null)
        {
            culture = CultureInfo.DefaultThreadCurrentCulture?.Name;
            language = _languages.FirstOrDefault(l => l.Culture == culture);
        }

        if (language == null)
        {
            language = _languages.First();
        }

        _language = language;
    }

    private void Save()
    {
        Settings.Default.Log_UseStylisticSet = _useStylisticSet;
        Settings.Default.Culture = _language.Culture;

        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(_language.Culture);
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(_language.Culture);

        Settings.Default.Save();
    }
}
