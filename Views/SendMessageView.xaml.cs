using System;
using System.Collections.Generic;
using System.Globalization;
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
/// Interaktionslogik für SendMessageView.xaml
/// </summary>
public partial class SendMessageView : UserControl
{
    public static readonly DependencyProperty SendCommandProperty =
        DependencyProperty.Register(
            nameof(SendCommand),
            typeof(ICommand),
            typeof(SendMessageView),
            new UIPropertyMetadata(null));

    public ICommand SendCommand
    {
        get => (ICommand)GetValue(SendCommandProperty);
        set => SetValue(SendCommandProperty, value);
    }

    public SendMessageView()
    {
        InitializeComponent();
    }

    private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        TextRow.MaxHeight = ActualHeight - OptionsPanel.ActualHeight - Row0.ActualHeight;
    }
}

public class YourConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return values.Clone();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return value as object[];
    }
}