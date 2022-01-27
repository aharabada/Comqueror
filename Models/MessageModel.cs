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
}
