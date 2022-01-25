using System;
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
    private byte[] _data;
    private MessageMode _messageMode;

    public byte[] Data
    {
        get => _data;
        set
        {
            if (SetIfChanged(ref _data, value))
            {

            }
        }
    }

    public string MessageText
    {
        get => _messageText;
        set => SetIfChanged(ref _messageText, value);
    }

    public MessageMode MessageMode
    {
        get => _messageMode;
        set => SetIfChanged(ref _messageMode, value);
    }

    public string FormatMessage(MessageFormat messageFormat)
    {
        StringBuilder stringBuilder = new();

        FormatAscii(stringBuilder);

        /*
        switch (messageFormat)
        {
            case MessageFormat.ASCII:
                FormatAscii(stringBuilder);
                break;
            case MessageFormat.Hex:
                FormatHex(stringBuilder);
                break;
        }
        */
        return stringBuilder.ToString();
    }

    private void FormatAscii(StringBuilder stringBuilder)
    {
        foreach (byte b in _data)
        {
            stringBuilder.Append((char)b);
        }
    }

    private void FormatHex(StringBuilder stringBuilder)
    {
        foreach (byte b in _data)
        {
            stringBuilder.Append($"0x{b:X2}");
        }
    }
}
