using Comqueror.Models;
using Comqueror.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Comqueror.ViewModels;

public class MainWindowViewModel : PropertyNotifier
{
    private ComPortModel _hostComPortSettings;
    private ComPortModel _deviceComPortSettings;

    private ComConnectionViewModel _hostComConnectionViewModel;
    private ComConnectionViewModel _deviceComConnectionViewModel;

    private bool _isHostConnected, _isDeviceConnected;

    private bool _forwardHostToDevice;
    private bool _forwardDeviceToHost;

    private SerialPort _hostComPort = new();
    private SerialPort _deviceComPort = new();

    public bool IsHostConnected
    {
        get => _isHostConnected;
        set => SetIfChanged(ref _isHostConnected, value);
    }

    public bool IsDeviceConnected
    {
        get => _isDeviceConnected;
        set => SetIfChanged(ref _isDeviceConnected, value);
    }

    public bool ForwardHostToDevice
    {
        get => _forwardHostToDevice;
        set => SetIfChanged(ref _forwardHostToDevice, value);
    }

    public bool ForwardDeviceToHost
    {
        get => _forwardDeviceToHost;
        set => SetIfChanged(ref _forwardDeviceToHost, value);
    }

    public ComPortModel HostComPortSettings
    {
        get => _hostComPortSettings;
        set => SetIfChanged(ref _hostComPortSettings, value);
    }

    public ComPortModel DeviceComPortSettings
    {
        get => _deviceComPortSettings;
        set => SetIfChanged(ref _deviceComPortSettings, value);
    }

    public ComConnectionViewModel HostComConnectionViewModel
    {
        get => _hostComConnectionViewModel;
        set => SetIfChanged(ref _hostComConnectionViewModel, value);
    }

    public ComConnectionViewModel DeviceComConnectionViewModel
    {
        get => _deviceComConnectionViewModel;
        set => SetIfChanged(ref _deviceComConnectionViewModel, value);
    }

    private RelayCommand? _connectHostCommand;
    private RelayCommand? _connectDeviceCommand;

    public RelayCommand ConnectHostCommand => _connectHostCommand ??=
        new((o) =>
        {
            IsHostConnected = Connect(_hostComPortSettings, _hostComPort);
        });
    public RelayCommand ConnectDeviceCommand => _connectDeviceCommand ??= 
        new((o) =>
        {
            IsDeviceConnected = Connect(_deviceComPortSettings, _deviceComPort);
        });

    public MainWindowViewModel()
    {
        HostComConnectionViewModel = new ComConnectionViewModel();
        DeviceComConnectionViewModel = new ComConnectionViewModel();

        _hostComPortSettings = new ComPortModel();
        _deviceComPortSettings = new ComPortModel();
        LoadSettings(_hostComPortSettings, _deviceComPortSettings);

        HostComConnectionViewModel.ComPortSettings = _hostComPortSettings;
        DeviceComConnectionViewModel.ComPortSettings = _deviceComPortSettings;
    }

    private struct SettingsContainer
    {
        public string ComPort;
        public int BaudRate;
        public string Parity;
        public int DataBits;
        public string StopBits;
        public string Handshake;

        public void FillComPortModel(ComPortModel comPortModel, IEnumerable<string> comPortNames)
        {
            if (!string.IsNullOrWhiteSpace(ComPort) && comPortNames.Contains(ComPort))
                comPortModel.PortName = ComPort;
            else
                comPortModel.PortName = comPortNames.FirstOrDefault();

            comPortModel.BaudRate = BaudRate;

            comPortModel.Parity = Enum.TryParse(Parity, out Parity parity) ? parity : System.IO.Ports.Parity.None;

            comPortModel.DataBits = DataBits;

            comPortModel.StopBits = Enum.TryParse(StopBits, out StopBits stopBits) ? stopBits : System.IO.Ports.StopBits.One;

            comPortModel.Handshake = Enum.TryParse(Handshake, out Handshake handshake) ? handshake : System.IO.Ports.Handshake.None;
        }
    }

    private void LoadSettings(ComPortModel hostComPortSettings, ComPortModel deviceComPortSettings)
    {
        SettingsContainer lastHostPortSettings = new()
        {
            ComPort = Settings.Default.HostComConnection_LastComPort,
            BaudRate = Settings.Default.HostComConnection_LastBaudRate,
            Parity = Settings.Default.HostComConnection_LastParity,
            DataBits = Settings.Default.HostComConnection_LastDataBits,
            StopBits = Settings.Default.HostComConnection_LastStoppBits,
            Handshake = Settings.Default.HostComConnection_LastHandshake
        };

        lastHostPortSettings.FillComPortModel(hostComPortSettings, HostComConnectionViewModel.PortNames);

        SettingsContainer lastDevicePortSettings = new()
        {
            ComPort = Settings.Default.DeviceComConnection_LastComPort,
            BaudRate = Settings.Default.DeviceComConnection_LastBaudRate,
            Parity = Settings.Default.DeviceComConnection_LastParity,
            DataBits = Settings.Default.DeviceComConnection_LastDataBits,
            StopBits = Settings.Default.DeviceComConnection_LastStoppBits,
            Handshake = Settings.Default.DeviceComConnection_LastHandshake
        };

        lastDevicePortSettings.FillComPortModel(deviceComPortSettings, DeviceComConnectionViewModel.PortNames);
    }

    private bool Connect(ComPortModel comPortSettings, SerialPort serialPort)
    {
        if (serialPort.IsOpen)
        {
            // Sollte der Port sich nicht schließen lassen -> RlComStreamAnalyzer.RlComControl.RlComControl.cs:void disConnect()
            serialPort.Close();

            return false;
        }

        if (string.IsNullOrWhiteSpace(comPortSettings.PortName))
        {
            MessageBox.Show("No Com Port selected.", "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);

            return false;
        }

        serialPort.PortName = comPortSettings.PortName;
        serialPort.BaudRate = comPortSettings.BaudRate;
        serialPort.Parity = comPortSettings.Parity;
        serialPort.DataBits = comPortSettings.DataBits;
        serialPort.StopBits = comPortSettings.StopBits;
        serialPort.Handshake = comPortSettings.Handshake;
        //serialPort.BreakState = ...
        // TODO: More settings

        try
        {
            serialPort.Open();

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);

            return false;
        }
    }
}
