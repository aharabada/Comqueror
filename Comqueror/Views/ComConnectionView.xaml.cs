using Comqueror.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Comqueror.Views;

/// <summary>
/// Interaktionslogik für ComConnectionView.xaml
/// </summary>
public partial class ComConnectionView : UserControl
{
    public static readonly DependencyProperty ConnectCommandProperty =
        DependencyProperty.Register(
            nameof(ConnectCommand),
            typeof(ICommand),
            typeof(ComConnectionView),
            new UIPropertyMetadata(null));

    public ICommand ConnectCommand
    {
        get => (ICommand)GetValue(ConnectCommandProperty);
        set => SetValue(ConnectCommandProperty, value);
    }

    public static readonly DependencyProperty ConnectionStateProperty =
        DependencyProperty.Register(
            nameof(ConnectionState),
            typeof(ConnectionState),
            typeof(ComConnectionView),
            new UIPropertyMetadata(null));
    public ConnectionState ConnectionState
    {
        get => (ConnectionState)GetValue(ConnectionStateProperty);
        set => SetValue(ConnectionStateProperty, value);
    }

    public ComConnectionView()
    {
        InitializeComponent();
    }
}
