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
    public static readonly DependencyProperty BlaConnectCommandProperty =
        DependencyProperty.Register(
            "BlaConnectCommand",
            typeof(ICommand),
            typeof(ComConnectionView),
            new UIPropertyMetadata(null));

    public ICommand BlaConnectCommand
    {
        get => (ICommand)GetValue(BlaConnectCommandProperty);
        set => SetValue(BlaConnectCommandProperty, value);
    }

    public static readonly DependencyProperty IsConnectedProperty =
        DependencyProperty.Register(
            "IsConnected",
            typeof(bool),
            typeof(ComConnectionView),
            new UIPropertyMetadata(null));
    public bool IsConnected
    {
        get => (bool)GetValue(IsConnectedProperty);
        set => SetValue(IsConnectedProperty, value);
    }

    public ComConnectionView()
    {
        InitializeComponent();
    }
}
