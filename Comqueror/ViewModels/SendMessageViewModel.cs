using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Comqueror.ViewModels;

public class SendMessageViewModel : PropertyNotifier
{
    private MessageType _messageType;

    public MessageType MessageType
    {
        get => _messageType;
        set => SetIfChanged(ref _messageType, value);
    }

    private string _message = string.Empty;

    public string Message
    {
        get => _message;
        set
        {
            SetIfChanged(ref _message, value);
        }
    }

    private bool _appendCarriageReturn;

    public bool AppendCarriageReturn
    {
        get => _appendCarriageReturn;
        set => SetIfChanged(ref _appendCarriageReturn, value);
    }

    private bool _appendNewLine;

    public bool AppendNewLine
    {
        get => _appendNewLine;
        set => SetIfChanged(ref _appendNewLine, value);
    }
}
