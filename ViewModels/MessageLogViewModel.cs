using Comqueror.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Data;

namespace Comqueror.ViewModels;

public class MessageLogViewModel : PropertyNotifier
{
    private readonly ObservableCollection<MessageViewModel> _messages = new();

    public ObservableCollection<MessageViewModel> Messages => _messages;

    public MessageViewModel _lastMessage;

    public readonly object MessagesLock = new();

    private int _messageBytesPerRow = 0;

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

    private RelayCommand _clearLogCommand;

    public RelayCommand ClearLogCommand => _clearLogCommand ??= new(o => ClearLog());

    private bool _autoScroll;

    public bool AutoScroll
    {
        get => _autoScroll;
        set => SetIfChanged(ref _autoScroll, value);
    }

    public MessageLogViewModel()
    {
        MessageViewModel vm = new();

        string msg = "Hallo Welt! Ich bin sooooo lang!!!!";

        vm.MessageModel = new()
        {
            MessageMode = MessageMode.Sent,
            Data = Encoding.ASCII.GetBytes(msg)
        };

        _messages.Add(vm);

        BindingOperations.EnableCollectionSynchronization(_messages, MessagesLock);
    }

    private void ClearLog()
    {
        lock (_messages)
        {
            _messages.Clear();
        }        
    }
    

    private void ReformatMessages()
    {
        foreach (MessageViewModel message in _messages)
        {
            message.ReformatMessage(_messageBytesPerRow);
        }
    }

    private void AddMessage(MessageModel message)
    {
        MessageViewModel messageViewModel = new();
        messageViewModel.MessageModel = message;
        messageViewModel.ReformatMessage(_messageBytesPerRow);

        lock (MessagesLock)
        {
            _messages.Add(messageViewModel);
        }
    }

    MessageMode _lastMode;
    MemoryStream _newDataBuffer = new();
    object _newDataBufferLock = new();

    internal void ReceivedData(byte[] data, MessageMode mode)
    {
        List<MessageModel> messages = new();

        void Flush()
        {
            if (_newDataBuffer.Length == 0)
                return;

            byte[] messageData = _newDataBuffer.ToArray();

            messages.Add(new MessageModel()
            {
                Data = messageData,
                MessageMode = _lastMode
            });

            _newDataBuffer.SetLength(0);
        }

        lock (_newDataBufferLock)
        {
            if (mode != _lastMode)
            {
                Flush();
            }

            ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(data);

            while (span.Length > 0)
            {
                int newLineIndex = span.IndexOf((byte)'\n');

                if (newLineIndex == -1)
                {
                    _newDataBuffer.Write(span);
                    break;
                }
                else
                {
                    ReadOnlySpan<byte> lineSlice = span.Slice(0, newLineIndex + 1);

                    _newDataBuffer.Write(lineSlice);

                    Flush();

                    // Cut data until newLine from Span
                    span = span.Slice(newLineIndex + 1);
                }
            }
        }

        if (messages.Count > 0)
        {
            foreach (MessageModel message in messages)
            {
                AddMessage(message);
            }
        }
    }
}
