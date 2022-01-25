using Comqueror.Models;
using System;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;

namespace Comqueror.ViewModels;

public class MainWindowViewModel : PropertyNotifier
{
    private ComPortModel _comPortSettings;

    private ComConnectionViewModel _comConnectionViewModel;

    private bool _isConnected;

    public bool IsConnected
    {
        get => _isConnected;
        set => SetIfChanged(ref _isConnected, value);
    }

    private SerialPort _comPort = new();

    public ComPortModel ComPortSettings
    {
        get => _comPortSettings;
        set => SetIfChanged(ref _comPortSettings, value);
    }

    public ComConnectionViewModel ComConnectionViewModel
    {
        get => _comConnectionViewModel;
        set => SetIfChanged(ref _comConnectionViewModel, value);
    }

    private RelayCommand? _connectCommand;

    public RelayCommand ConnectCommand => _connectCommand ??= new(async (o) => await Connect(o));

    public MainWindowViewModel()
    {
        ComConnectionViewModel = new ComConnectionViewModel();

        _comPortSettings = new ComPortModel();
        LoadSettings(_comPortSettings);

        ComConnectionViewModel.ComPortSettings = _comPortSettings;
    }

    private void LoadSettings(ComPortModel comPortSettings)
    {
        string? lastComPort = Properties.Settings.Default.ComConnection_LastComPort;

        if (!string.IsNullOrWhiteSpace(lastComPort) && ComConnectionViewModel.PortNames.Contains(lastComPort))
        {
            comPortSettings.PortName = lastComPort;
        }
        else
        {
            comPortSettings.PortName = ComConnectionViewModel.PortNames[0];
        }

        int lastBaudRate = Properties.Settings.Default.ComConnection_LastBaudRate;
        comPortSettings.BaudRate = lastBaudRate;
    }

    private async Task Connect(object o)
    {
        if (_comPort.IsOpen)
        {
            _comPort.Close();
            IsConnected = false;
            //ComConnectionViewModel.IsConnected = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(_comPortSettings.PortName))
        {
            MessageBox.Show("No Com Port selected.", "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        _comPort.PortName = "COM1";//_comPortSettings.PortName;
        _comPort.BaudRate = _comPortSettings.BaudRate;
        // TODO: More settings

        try
        {
            _comPort.Open();
            IsConnected = true;
            //ComConnectionViewModel.IsConnected = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
