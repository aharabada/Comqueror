using System.Collections.ObjectModel;
using System.Text;

namespace Comqueror.Models;

public enum MessageMode
{
    Received,
    Sent
}

public class MessageModel : PropertyNotifier
{
    private string? _messageText;
    private string? _messageHex;
    private byte[] _data;
    private MessageMode _messageMode;

    public byte[] Data
    {
        get => _data;
        set
        {
            if (SetIfChanged(ref _data, value))
            {
                MessageText = FormatMessage(false);
                MessageHex = FormatMessage(true);
            }
        }
    }

    public string MessageText
    {
        get => _messageText;
        set => SetIfChanged(ref _messageText, value);
    }
    public string MessageHex
    {
        get => _messageHex;
        set => SetIfChanged(ref _messageHex, value);
    }

    public MessageMode MessageMode
    {
        get => _messageMode;
        set => SetIfChanged(ref _messageMode, value);
    }

    public string FormatMessage()
    {
        StringBuilder stringBuilder = new();

        FormatHex(stringBuilder);

        stringBuilder.Append("  <-->  ");

        FormatAscii(stringBuilder);

        return stringBuilder.ToString();
    }
    public string FormatMessage(bool hex)
    {
        StringBuilder stringBuilder = new();

        if (hex)
        {
            FormatHex(stringBuilder);
        }
        else
        {
            FormatAscii(stringBuilder);
        }

        return stringBuilder.ToString();
    }

    private void FormatAscii(StringBuilder stringBuilder)
    {
        int i = 0;

        foreach (byte b in _data)
        {
            stringBuilder.Append((char)b);
            i++;
        }
        for (; i < 8; i++)
        {
            stringBuilder.Append(".");
        }
    }

    private void FormatHex(StringBuilder stringBuilder)
    {
        int i = 0;

        foreach (byte b in _data)
        {
            stringBuilder.Append($"{b:X2} ");
            i++;
        }

        for (; i < 8; i++)
        {
            stringBuilder.Append("__ ");
        }
    }
}
