using Comqueror.Models;
using Comqueror.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RJCP.IO.Ports;
using System.Windows.Threading;

namespace Comqueror.ViewModels;

public class MainWindowViewModel : PropertyNotifier
{
    private ComPortModel _hostComPortSettings;
    private ComPortModel _deviceComPortSettings;

    private ComConnectionViewModel _hostComConnectionViewModel = new();
    private ComConnectionViewModel _deviceComConnectionViewModel = new();

    private readonly MessageLogViewModel _messageLogViewModel = new();

    private ConnectionState _hostConnectionState, _deviceConnectionState;

    private bool _forwardHostToDevice;
    private bool _forwardDeviceToHost;

    private SerialPortStream? _hostComPortStream;
    private SerialPortStream? _deviceComPortStream;

    public MessageLogViewModel MessageLogViewModel => _messageLogViewModel;

    public ConnectionState HostConnectionState
    {
        get => _hostConnectionState;
        set => SetIfChanged(ref _hostConnectionState, value);
    }

    public ConnectionState DeviceConnectionState
    {
        get => _deviceConnectionState;
        set => SetIfChanged(ref _deviceConnectionState, value);
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
        new(async (o) => await ConnectOrDisconnectHost());

    private static async Task DisconnectAsync(SerialPortStream? port)
    {
        if (port == null)
            return;

        port.Close();
        await port.DisposeAsync();
    }

    public RelayCommand ConnectDeviceCommand => _connectDeviceCommand ??= 
        new(async (o) => await ConnectOrDisconnectDevice());
    private async Task ConnectOrDisconnectDevice(bool showErrors = true)
    {
        if (DeviceConnectionState != ConnectionState.Disconnected)
        {
            await DisconnectDevice();
        }
        else
        {
            await ConnectDevice(showErrors);
        }
    }

    private async Task DisconnectDevice()
    {
        DeviceConnectionState = ConnectionState.Diconnecting;

        await DisconnectAsync(_deviceComPortStream);
        _deviceComPortStream = null;

        DeviceConnectionState = ConnectionState.Disconnected;
    }

    private async Task<bool> ConnectDevice(bool showErrors)
    {
        if (DeviceConnectionState != ConnectionState.Reconnecting)
            DeviceConnectionState = ConnectionState.Connecting;

        _deviceComPortStream = new SerialPortStream();
        if (await ConnectAsync(_deviceComPortSettings, _deviceComPortStream, showErrors))
        {
            RegisterDevicePortEvents(_deviceComPortStream);

            DeviceConnectionState = ConnectionState.Connected;

            return true;
        }
        else
        {
            if (DeviceConnectionState != ConnectionState.Reconnecting)
                DeviceConnectionState = ConnectionState.Disconnected;

            return false;
        }
    }

    private async Task ConnectOrDisconnectHost(bool showErrors = true)
    {
        if (HostConnectionState != ConnectionState.Disconnected)
        {
            await DisconnectHost();
        }
        else
        {
            await ConnectHost(showErrors);
        }
    }

    private async Task DisconnectHost()
    {
        HostConnectionState = ConnectionState.Diconnecting;

        await DisconnectAsync(_hostComPortStream);
        _hostComPortStream = null;

        HostConnectionState = ConnectionState.Disconnected;
    }

    private async Task<bool> ConnectHost(bool showErrors)
    {
        if (HostConnectionState != ConnectionState.Reconnecting)
            HostConnectionState = ConnectionState.Connecting;

        _hostComPortStream = new SerialPortStream();

        if (await ConnectAsync(_hostComPortSettings, _hostComPortStream, showErrors))
        {
            RegisterHostPortEvents(_hostComPortStream);

            HostConnectionState = ConnectionState.Connected;

            return true;
        }
        else
        {
            if (HostConnectionState != ConnectionState.Reconnecting)
                HostConnectionState = ConnectionState.Disconnected;

            return false;
        }
    }


    public RelayCommand SendMessageCommand => _sendMessageCommand ??=
        new(async o => await SendMessageAsync(o));

    private async Task SendMessageAsync(object? parameters)
    {
        await Task.Run(() =>
        {
            if (_deviceComPortStream == null)
            {
                MessageBox.Show(Strings.Error_NotConnected);
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();

            object[]? param = (object[]?)parameters;

            if (param == null || param.Length != 4)
                return;

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

                    if (!byte.TryParse(split, System.Globalization.NumberStyles.HexNumber, null, out data[i]))
                    {
                        string error = string.Format(Strings.Error_InvalidHexNumber, split);

                        MessageBox.Show(error);

                        return;
                    }
                }
            }

            if (data == null)
                return;

            _deviceComPortStream.Write(data, 0, data.Length);

            MessageLogViewModel.LogData(data, MessageMode.Sent);

            sw.Stop();

            Debug.WriteLine($"{sw.Elapsed.TotalMilliseconds} ms");
        });
    }

    private void RegisterHostPortEvents(SerialPortStream hostComPort)
    {
        hostComPort.DataReceived += async (s, e) =>
        {
            SerialPortStream? port = (SerialPortStream?)s;

            if (port == null)
                return;

            int bytesAvailable = port.BytesToRead;
            byte[] data = new byte[bytesAvailable];

            int i = await port.ReadAsync(data);

            Debug.WriteLine($"Read {i} bytes from Host.");

            while ((bytesAvailable = port.BytesToRead) > 0)
            {
                int oldLength = data.Length;

                Array.Resize(ref data, data.Length + bytesAvailable);

                i = await port.ReadAsync(data, oldLength, bytesAvailable);

                Debug.WriteLine($"Read additional {i} bytes from Host.");
            }

            MessageLogViewModel.LogData(data, MessageMode.Sent);

            if (ForwardHostToDevice && port.IsOpen)
            {
                port.Write(data, 0, data.Length);
            }
        };
    }

    private void RegisterDevicePortEvents(SerialPortStream deviceComPort)
    {
        deviceComPort.DataReceived += async (s, e) =>
        {
            SerialPortStream? port = (SerialPortStream?)s;

            if (port == null)
                return;

            int bytesAvailable = port.BytesToRead;
            byte[] data = new byte[bytesAvailable];

            int i = await port.ReadAsync(data);

            Debug.WriteLine($"Read {i} bytes from Device.");

            while ((bytesAvailable = port.BytesToRead) > 0)
            {
                int oldLength = data.Length;

                Array.Resize(ref data, data.Length + bytesAvailable);

                i = await port.ReadAsync(data, oldLength, bytesAvailable);

                Debug.WriteLine($"Read additional {i} bytes from Device.");
            }

            MessageLogViewModel.LogData(data, MessageMode.Received);

            if (ForwardDeviceToHost && port.IsOpen)
            {
                port.Write(data, 0, data.Length);
            }
        };
    }

    private readonly DispatcherTimer _checkAliveTimer;

    private bool _automaticReconnect = true;

    public bool AutomaticReconnect
    {
        get => _automaticReconnect;
        set => SetIfChanged(ref _automaticReconnect, value);
    }

    public MainWindowViewModel()
    {
        _hostComPortSettings = new ComPortModel();
        _deviceComPortSettings = new ComPortModel();
        LoadSettings(_hostComPortSettings, _deviceComPortSettings);

        HostComConnectionViewModel.ComPortSettings = _hostComPortSettings;
        DeviceComConnectionViewModel.ComPortSettings = _deviceComPortSettings;

        _checkAliveTimer = new DispatcherTimer();
        _checkAliveTimer.Interval = TimeSpan.FromSeconds(1);
        _checkAliveTimer.Tick += async (s, e) => await CheckComPorts(s, e);
        _checkAliveTimer.Start();
    }

    bool tryReconnectDevice = false;
    bool tryReconnectHost = false;

    int _deviceReconnectAttempts = 0;
    int _hostReconnectAttempts = 0;

    public int DeviceReconnectAttempts
    {
        get => _deviceReconnectAttempts;
        set => SetIfChanged(ref _deviceReconnectAttempts, value);
    }
    public int HostReconnectAttempts
    {
        get => _hostReconnectAttempts;
        set => SetIfChanged(ref _hostReconnectAttempts, value);
    }

    private async Task CheckComPorts(object? s, EventArgs e)
    {
        if (!tryReconnectDevice && DeviceConnectionState != ConnectionState.Disconnected && _deviceComPortStream?.IsOpen != true)
        {
            try
            {
                await DisconnectDevice();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            tryReconnectDevice = AutomaticReconnect;
            DeviceReconnectAttempts = 0;
        }

        if (tryReconnectDevice)
        {
            if (DeviceConnectionState == ConnectionState.Connected)
            {
                tryReconnectDevice = false;
            }
            else
            {
                DeviceReconnectAttempts++;
                DeviceConnectionState = ConnectionState.Reconnecting;

                await ConnectDevice(false);
            }
        }

        if (!tryReconnectHost && HostConnectionState != ConnectionState.Disconnected && _hostComPortStream?.IsOpen != true)
        {
            try
            {
                await DisconnectHost();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            tryReconnectHost = AutomaticReconnect;
            HostReconnectAttempts = 0;
        }

        if (tryReconnectHost)
        {
            if (HostConnectionState == ConnectionState.Connected)
            {
                tryReconnectHost = false;
            }
            else
            {
                HostReconnectAttempts++;
                HostConnectionState = ConnectionState.Reconnecting;

                await ConnectHost(false);
            }
        }
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

            comPortModel.Parity = Enum.TryParse(Parity, out Parity parity) ? parity : RJCP.IO.Ports.Parity.None;

            comPortModel.DataBits = DataBits;

            comPortModel.StopBits = Enum.TryParse(StopBits, out StopBits stopBits) ? stopBits : RJCP.IO.Ports.StopBits.One;

            comPortModel.Handshake = Enum.TryParse(Handshake, out Handshake handshake) ? handshake : RJCP.IO.Ports.Handshake.None;
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

    private static Task<bool> ConnectAsync(ComPortModel comPortSettings, SerialPortStream serialPort, bool showErrors)
    {
        return Task.Run(() => Connect(comPortSettings, serialPort, showErrors));
    }

    private static bool Connect(ComPortModel comPortSettings, SerialPortStream serialPort, bool showErrors)
    {
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
            if (showErrors)
                MessageBox.Show(ex.Message, "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);

            Debug.WriteLine($"Failed to connect: {ex.Message}");

            return false;
        }
    }
}
