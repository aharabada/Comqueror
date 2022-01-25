using System.IO.Ports;

namespace Comqueror.Models;

public class ComPortModel : PropertyNotifier
{
    private string? _portName;
    private int _baudRate;
    private Parity _parity;
    private StopBits _stopBits;
    private int _dataBits;
    private Handshake _handshake;


    public string? PortName
    {
        get => _portName;
        set => SetIfChanged(ref _portName, value);
    }

    public int BaudRate
    {
        get => _baudRate;
        set => SetIfChanged(ref _baudRate, value);
    }

    public Parity Parity
    {
        get => _parity;
        set => SetIfChanged(ref _parity, value);
    }

    public StopBits StopBits
    {
        get => _stopBits;
        set => SetIfChanged(ref _stopBits, value);
    }

    public int DataBits
    {
        get => _dataBits;
        set => SetIfChanged(ref _dataBits, value);
    }

    public Handshake Handshake
    {
        get => _handshake;
        set => SetIfChanged(ref _handshake, value);
    }
}
