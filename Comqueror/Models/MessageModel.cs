using System.Collections.ObjectModel;
using System.Text;

namespace Comqueror.Models;

public enum MessageMode
{
    None,
    Received,
    Sent,
}

public class MessageModel : PropertyNotifier
{
    private byte[] _data;
    private MessageMode _messageMode;

    private int _messageIndex;

    public byte[] Data
    {
        get => _data;
        set
        {
            SetIfChanged(ref _data, value);
        }
    }

    public MessageMode MessageMode
    {
        get => _messageMode;
        set => SetIfChanged(ref _messageMode, value);
    }

    public int MessageIndex
    {
        get => _messageIndex;
        set => SetIfChanged(ref _messageIndex, value);
    }
}
