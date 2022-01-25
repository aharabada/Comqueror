using System;
using System.Collections.Generic;
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
/// Interaktionslogik für MessageLogView.xaml
/// </summary>
public partial class MessageLogView : UserControl
{
    public MessageLogView()
    {
        InitializeComponent();
    }
}

public class DatetimeTemplateSelector : DataTemplateSelector
{
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        FrameworkElement? element = container as FrameworkElement;

        //if (element != null && item != null && item is SettingViewModel model)
        //{
        //    return (DataTemplate)element.FindResource(model.EditElement.ToString() + "Template");
        //}

        return null;
    }
}