using System.Collections.ObjectModel;
using System.Text;

namespace Comqueror.ViewModels;

public class MessageLogViewModel : PropertyNotifier
{
    private readonly ObservableCollection<MessageViewModel> _messages = new();

    public ObservableCollection<MessageViewModel> Messages => _messages;

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
            MessageMode = Models.MessageMode.Sent,
            Data = Encoding.ASCII.GetBytes(msg)
        };

        _messages.Add(vm);
    }

    private void ClearLog() => _messages.Clear();

    private void ReformatMessages()
    {
        foreach (MessageViewModel message in _messages)
        {
            message.ReformatMessage(_messageBytesPerRow);
        }
    }
}
