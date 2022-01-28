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

    private MessageViewModel _lastMessage;

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

    public RelayCommand ClearLogCommand => _clearLogCommand ??= new(async o => await Task.Run(ClearLog));

    public MessageLogViewModel()
    {
        string msg = "Hallo Welt! Ich bin sooooo lang!!!!";

        MessageModel msgMod = new()
        {
            MessageMode = MessageMode.Sent,
            Data = Encoding.ASCII.GetBytes(msg)
        };

        AddMessage(msgMod, true);

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
                AddMessage(new MessageModel()
                {
                    Data = new byte[0]
                }, false);
            }
            else
            {
                _lastMessage = messageViewModel;
            }
        }
    }

    int _receivedMessages = 0;
    int _sentMessages = 0;

    internal void LogData(byte[] data, MessageMode mode)
    {
        if (data.Length == 0)
            return;

        void AppendSpan(MessageModel message, ReadOnlySpan<byte> data)
        {
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
            if (_lastMessage.MessageModel.MessageMode == MessageMode.None)
                _lastMessage.MessageModel.MessageMode = mode;

            if (_lastMessage.MessageModel.MessageMode != mode)
                AddMessage(new MessageModel()
                {
                    Data = new byte[0]
                }, false);

            MessageModel message = _lastMessage.MessageModel;

            ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(data);

            while (span.Length > 0)
            {
                int newLineIndex = span.IndexOf((byte)'\n');

                if (newLineIndex == -1)
                {
                    AppendSpan(message, span);

                    break;
                }
                else
                {
                    ReadOnlySpan<byte> lineSlice = span.Slice(0, newLineIndex + 1);

                    AppendSpan(message, lineSlice);

                    if (mode == MessageMode.Sent)
                    {
                        message.MessageIndex = ++_sentMessages;
                    }
                    else if (mode == MessageMode.Received)
                    {
                        message.MessageIndex = ++_receivedMessages;
                    }

                    AddMessage(new MessageModel()
                    {
                        Data = new byte[0]
                    }, false);
                    message = _lastMessage.MessageModel;

                    // Cut data until newLine from Span
                    span = span.Slice(newLineIndex + 1);
                }
            }
        }

        //List<MessageModel> messages = new();

        //void Flush()
        //{
        //    if (_newDataBuffer.Length == 0)
        //        return;

        //    byte[] messageData = _newDataBuffer.ToArray();

        //    messages.Add(new MessageModel()
        //    {
        //        Data = messageData,
        //        MessageMode = _lastMode
        //    });

        //    _newDataBuffer.SetLength(0);
        //}

        //lock (_newDataBufferLock)
        //{
        //    if (mode != _lastMode)
        //    {
        //        Flush();
        //    }

        //    ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(data);

        //    while (span.Length > 0)
        //    {
        //        int newLineIndex = span.IndexOf((byte)'\n');

        //        if (newLineIndex == -1)
        //        {
        //            _newDataBuffer.Write(span);
        //            break;
        //        }
        //        else
        //        {
        //            ReadOnlySpan<byte> lineSlice = span.Slice(0, newLineIndex + 1);

        //            _newDataBuffer.Write(lineSlice);

        //            Flush();

        //            // Cut data until newLine from Span
        //            span = span.Slice(newLineIndex + 1);
        //        }
        //    }
        //}

        //if (messages.Count > 0)
        //{
        //    foreach (MessageModel message in messages)
        //    {
        //        AddMessage(message);
        //    }
        //}
    }
}
