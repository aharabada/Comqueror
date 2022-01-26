using Comqueror.Models;
using System.Collections.ObjectModel;

namespace Comqueror.ViewModels;


public class MessageLogViewModel : PropertyNotifier
{
    private readonly ObservableCollection<MessageModel> _messages = new()
    {
        new() { Data = new byte[] { (byte)'H', (byte)'i' }, MessageMode=MessageMode.Sent },
        new() { Data = new byte[] { (byte)'H', (byte)'a', (byte)'l', (byte)'l', (byte)'o' }, MessageMode = MessageMode.Received }
    };

    public ObservableCollection<MessageModel> Messages => _messages;

    private double _hexHeaderWidth;

    public double HexHeaderWidth
    {
        get => _hexHeaderWidth;
        set => SetIfChanged(ref _hexHeaderWidth, value);
    }
}
