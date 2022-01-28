using Comqueror.Models;
using Comqueror.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Comqueror.ViewModels;

public class MainWindowViewModel : PropertyNotifier
{
    private ComPortModel _hostComPortSettings;
    private ComPortModel _deviceComPortSettings;

    private ComConnectionViewModel _hostComConnectionViewModel;
    private ComConnectionViewModel _deviceComConnectionViewModel;

    private MessageLogViewModel _messageLogViewModel = new();

    private bool _isHostConnected, _isDeviceConnected;

    private bool _forwardHostToDevice;
    private bool _forwardDeviceToHost;

    private SerialPort _hostComPort = new();
    private SerialPort _deviceComPort = new();

    public MessageLogViewModel MessageLogViewModel => _messageLogViewModel;

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

    private RelayCommand? _sendMessageCommand;

    public RelayCommand ConnectHostCommand => _connectHostCommand ??=
        new(async (o) =>
        {
            IsHostConnected = await ConnectAsync(_hostComPortSettings, _hostComPort);
            if (IsHostConnected)
                RegisterHostPortEvents(_hostComPort);
        });

    public RelayCommand ConnectDeviceCommand => _connectDeviceCommand ??= 
        new(async (o) => 
        {
            IsDeviceConnected = await ConnectAsync(_deviceComPortSettings, _deviceComPort);
            if (IsDeviceConnected)
                RegisterDevicePortEvents(_deviceComPort);
        });

    public RelayCommand SendMessageCommand => _sendMessageCommand ??=
        new(async o => await SendMessageAsync(o));

    private async Task SendMessageAsync(object parameters)
    {
        await Task.Run(() =>
        {
            Stopwatch sw = Stopwatch.StartNew();

            object[] param = (object[])parameters;

            string message = (string)param[0];
            MessageType messageType = (MessageType)param[1];
            bool appendCarriageReturn = (bool)param[2];
            bool appendNewLine = (bool)param[3];

            if (appendCarriageReturn)
                message += "\r";

            if (appendNewLine)
                message += "\n";

            byte[]? data = null;

            if (messageType == MessageType.Ascii)
            {
                data = Encoding.ASCII.GetBytes(message);
            }
            else if (messageType == MessageType.Hex)
            {
                string[] splits = message.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                data = new byte[splits.Length];

                for (int i = 0; i < splits.Length; i++)
                {
                    string split = splits[i];

                    if (split.Length != 2)
                    {
                        MessageBox.Show(Strings.Error_HexNumberTooLong);

                        return;
                    }

                    if (byte.TryParse(split, out data[i]))
                    {
                        string error = string.Format(Strings.Error_InvalidHexNumber, split);

                        MessageBox.Show(error);

                        return;
                    }
                }
            }

            if (data == null)
                return;

            _deviceComPort.Write(data, 0, data.Length);

            MessageLogViewModel.LogData(data, MessageMode.Sent);

            sw.Stop();

            Debug.WriteLine($"{sw.Elapsed.TotalMilliseconds} ms");
        });
    }

    private void RegisterHostPortEvents(SerialPort hostComPort)
    {
        hostComPort.DataReceived += async (s, e) =>
        {
            SerialPort port = (SerialPort)s;

            int bytesAvailable = port.BytesToRead;
            byte[] data = new byte[bytesAvailable];

            int i = await port.BaseStream.ReadAsync(data);

            Debug.WriteLine($"Read {i} bytes from Host.");

            while ((bytesAvailable = port.BytesToRead) > 0)
            {
                int oldLength = data.Length;

                Array.Resize(ref data, data.Length + bytesAvailable);

                i = await port.BaseStream.ReadAsync(data, oldLength, bytesAvailable);

                Debug.WriteLine($"Read additional {i} bytes from Host.");
            }

            MessageLogViewModel.LogData(data, MessageMode.Sent);

            if (ForwardHostToDevice && _deviceComPort.IsOpen)
            {
                _deviceComPort.Write(data, 0, data.Length);
            }
        };
    }

    private void RegisterDevicePortEvents(SerialPort deviceComPort)
    {
        deviceComPort.DataReceived += async (s, e) =>
        {
            SerialPort port = (SerialPort)s;

            int bytesAvailable = port.BytesToRead;
            byte[] data = new byte[bytesAvailable];

            int i = await port.BaseStream.ReadAsync(data);

            Debug.WriteLine($"Read {i} bytes from Device.");

            while ((bytesAvailable = port.BytesToRead) > 0)
            {
                int oldLength = data.Length;

                Array.Resize(ref data, data.Length + bytesAvailable);

                i = await port.BaseStream.ReadAsync(data, oldLength, bytesAvailable);

                Debug.WriteLine($"Read additional {i} bytes from Device.");
            }

            MessageLogViewModel.LogData(data, MessageMode.Received);

            if (ForwardDeviceToHost && _hostComPort.IsOpen)
            {
                _hostComPort.Write(data, 0, data.Length);
            }
        };
    }

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

    private Task<bool> ConnectAsync(ComPortModel comPortSettings, SerialPort serialPort)
    {
        return Task.Run(() => Connect(comPortSettings, serialPort));
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

        serialPort.DtrEnable = true;
        serialPort.RtsEnable = true;

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
