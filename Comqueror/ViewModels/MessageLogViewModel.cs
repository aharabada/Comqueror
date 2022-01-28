using Comqueror.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Comqueror.ViewModels;

public class MessageLogViewModel : PropertyNotifier
{
    private readonly ObservableCollection<MessageViewModel> _messages = new();

    public ObservableCollection<MessageViewModel> Messages => _messages;

    private MessageViewModel _lastMessage = new();

    public readonly object MessagesLock = new();

    private int _messageBytesPerRow = 0;

    int _receivedMessages = 0;
    int _sentMessages = 0;

    public int MessageBytesPerRow
    {
        get => _messageBytesPerRow;
        set
        {
            if (SetIfChanged(ref _messageBytesPerRow, value))
            {
                ReformatMessages();
            }
        }
    }

    private RelayCommand? _clearLogCommand;

    public RelayCommand ClearLogCommand => _clearLogCommand ??= new(async o => await Task.Run(ClearLog));

    public MessageLogViewModel()
    {
        BindingOperations.EnableCollectionSynchronization(_messages, MessagesLock);

        AddEmptyMessage();
    }

    private void ClearLog()
    {
        lock (_messages)
        {
            _receivedMessages = 0;
            _sentMessages = 0;
            _messages.Clear();
            AddEmptyMessage();
        }  
    }
    
    private void AddEmptyMessage()
    {
        AddMessage(new MessageModel(), false);
    }

    private void ReformatMessages()
    {
        lock (MessagesLock)
        {
            foreach (MessageViewModel message in _messages)
            {
                message.ReformatMessage(_messageBytesPerRow);
            }
        }
    }

    private void AddMessage(MessageModel message, bool newLastMessage)
    {
        MessageViewModel messageViewModel = new();
        messageViewModel.MessageModel = message;
        messageViewModel.ReformatMessage(_messageBytesPerRow);

        lock (MessagesLock)
        {
            _messages.Add(messageViewModel);

            if (newLastMessage)
            {
                AddEmptyMessage();
            }
            else
            {
                _lastMessage = messageViewModel;
            }
        }
    }

    internal void LogData(byte[] data, MessageMode mode)
    {
        if (data.Length == 0)
            return;

        void AppendSpan(ReadOnlySpan<byte> data)
        {
            MessageModel message = _lastMessage.MessageModel;

            int oldLength = message.Data.Length;

            byte[] msgData = message.Data;

            Array.Resize(ref msgData, oldLength + data.Length);

            data.CopyTo(new Span<byte>(msgData, oldLength, data.Length));

            message.Data = msgData;

            if (message.MessageMode == MessageMode.None)
            {
                message.MessageMode = mode;

                if (mode == MessageMode.Sent)
                {
                    message.MessageIndex = ++_sentMessages;
                }
                else if (mode == MessageMode.Received)
                {
                    message.MessageIndex = ++_receivedMessages;
                }
            }

            _lastMessage.ReformatMessage(_messageBytesPerRow);
        }

        lock (MessagesLock)
        {
            if (_lastMessage.MessageModel.MessageMode != MessageMode.None && 
                _lastMessage.MessageModel.MessageMode != mode)
                AddEmptyMessage();

            ReadOnlySpan<byte> span = new(data);

            while (span.Length > 0)
            {
                int newLineIndex = span.IndexOf((byte)'\n');

                if (newLineIndex == -1)
                {
                    AppendSpan(span);

                    break;
                }
                else
                {
                    ReadOnlySpan<byte> lineSlice = span.Slice(0, newLineIndex + 1);

                    AppendSpan(lineSlice);

                    AddEmptyMessage();

                    // Cut line from Span
                    span = span.Slice(newLineIndex + 1);
                }
            }
        }
    }
}
