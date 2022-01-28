using Comqueror.Models;
using System.Text;

namespace Comqueror.ViewModels;

public class MessageViewModel : PropertyNotifier
{
    private MessageModel _messageModel = new();

    public MessageModel MessageModel
    {
        get => _messageModel;
        set => SetIfChanged(ref _messageModel, value);
    }

    private string? _asciiMessage;
    private string? _hexMessage;

    public string? HexMessage
    {
        get => _hexMessage;
        set => SetIfChanged(ref _hexMessage, value);
    }

    public string? AsciiMessage
    {
        get => _asciiMessage;
        set => SetIfChanged(ref _asciiMessage, value);
    }

    public void ReformatMessage(int bytesPerRow)
    {
        AsciiMessage = FormatMessage(false, bytesPerRow);
        HexMessage = FormatMessage(true, bytesPerRow);
    }

    public string FormatMessage(bool hex, int bytesPerRow)
    {
        StringBuilder stringBuilder = new();

        if (hex)
        {
            FormatHex(stringBuilder, bytesPerRow);
        }
        else
        {
            FormatAscii(stringBuilder, bytesPerRow);
        }

        return stringBuilder.ToString();
    }

    private void FormatAscii(StringBuilder stringBuilder, int bytesPerRow)
    {
        if (_messageModel.Data == null)
            return;

        int i = 0;

        foreach (byte b in _messageModel.Data)
        {
            stringBuilder.Append((char)b);

            if (++i == bytesPerRow)
            {
                stringBuilder.AppendLine();
                i = 0;
            }
        }

        // Fill row with .
        for (; i < bytesPerRow; i++)
        {
            stringBuilder.Append('.');
        }
    }

    private void FormatHex(StringBuilder stringBuilder, int bytesPerRow)
    {
        if (_messageModel.Data == null)
            return;

        int i = 0;

        foreach (byte b in _messageModel.Data)
        {
            stringBuilder.Append($"{b:X2} ");

            if (++i == bytesPerRow)
            {
                stringBuilder.AppendLine();
                i = 0;
            }
        }

        // Fill row with __
        for (; i < bytesPerRow; i++)
        {
            stringBuilder.Append("__ ");
        }
    }
}
