using Comqueror.Models;
using RJCP.IO.Ports;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Windows.Input;

namespace Comqueror.ViewModels;

//public struct BaudrateModel
//{
//    public int Baudrate { get; init; }

//    public static implicit operator BaudrateModel(int baudRate)
//    {
//        return new()
//        {
//            Baudrate = baudRate
//        };
//    }
//}

public class ComConnectionViewModel : PropertyNotifier
{
    private ObservableCollection<string> _portNames = new();

    private ObservableCollection<int> _baudRates = new()
    {
        300,
        600,
        1200,
        2400,
        4800,
        9600,
        19_200,
        38_400,
        57_600,
        74_880,
        115_200,
        230_400,
        250_000,
        500_000,
        1_000_000,
        2_000_000
    };

    private readonly List<int> _dataBits = new() { 5, 6, 7, 8 };

    public List<int> AvailableDataBits => _dataBits;

    private ComPortModel? _comPortSettings;

    public ComPortModel? ComPortSettings
    {
        get => _comPortSettings;
        set => SetIfChanged(ref _comPortSettings, value);
    }

    public ObservableCollection<string> PortNames
    {
        get => _portNames;
        set => SetIfChanged(ref _portNames, value);
    }

    public ObservableCollection<int> BaudRates
    {
        get => _baudRates;
        set => SetIfChanged(ref _baudRates, value);
    }

    private RelayCommand? _updateComPortsCommand;

    public RelayCommand UpdateComPortsCommand => _updateComPortsCommand ??= new(o => UpdateComPorts());

    public ComConnectionViewModel()
    {
        UpdateComPorts();
    }

    /// <summary>
    /// Updates <see cref="PortNames">.
    /// </summary>
    private void UpdateComPorts()
    {
        string[] portNames = SerialPortStream.GetPortNames();

        for (int i = 0; i < _portNames.Count; i++)
        {
            if (!portNames.Contains(_portNames[i]))
            {
                _portNames.RemoveAt(i);
                i--;
            }
        }

        foreach (string portName in portNames)
        {
            if (!_portNames.Contains(portName))
                _portNames.Add(portName);
        }
    }
}
