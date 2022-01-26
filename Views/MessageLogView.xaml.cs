using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
/// Interaktionslogik für MessageLogView.xaml
/// </summary>
public partial class MessageLogView : UserControl
{
    TextBlock? HexHeaderTextBlock;
    Rectangle? HexHeaderRect;

    TextBlock? AsciiHeaderTextBlock;
    Rectangle? AsciiHeaderRect;

    public MessageLogView()
    {
        InitializeComponent();
    }

    private void HexHeaderTextBlockLoaded(object sender, RoutedEventArgs e) => HexHeaderTextBlock = sender as TextBlock;

    private void AsciiHeaderTextBlockLoaded(object sender, RoutedEventArgs e) => AsciiHeaderTextBlock = sender as TextBlock;

    private void HexHeaderRectLoaded(object sender, RoutedEventArgs e) => HexHeaderRect = sender as Rectangle;

    private void AsciiHeaderRectLoaded(object sender, RoutedEventArgs e) => AsciiHeaderRect = sender as Rectangle;

    private void MessageHeaderSizeChanged(object sender, SizeChangedEventArgs e)
    {
        FindBestCharsPerRow();
    }

    int lastNums = 0;

    private void FindBestCharsPerRow()
    {
        if (HexHeaderTextBlock == null || AsciiHeaderTextBlock == null || HexHeaderRect == null || AsciiHeaderRect == null)
            return;

        int hexNums = FindHexNumsPerRow(HexHeaderRect.ActualWidth);

        int charNums = FindAsciiCharsPerRow(AsciiHeaderRect.ActualWidth);

        int actualNums = Math.Min(hexNums, charNums);

        if (actualNums != lastNums)
        {
            StringBuilder builder = new();

            for (int i = 0; i < actualNums; i++)
            {
                builder.AppendFormat("{0,-3:X}", i);
            }

            HexHeaderTextBlock.Text = builder.ToString();

            builder.Clear();

            for (int i = 0; i < actualNums; i++)
            {
                builder.AppendFormat("{0:X}", i % 16);
            }

            AsciiHeaderTextBlock.Text = builder.ToString();
        }
    }

    private int FindHexNumsPerRow(double width)
    {
        int hexTargetNums = 0;

        string? header = null;

        StringBuilder builder = new();

        while (true)
        {
            int newTarget = hexTargetNums + 8;

            for (int hexNums = hexTargetNums; hexNums < newTarget; hexNums++)
            {
                builder.AppendFormat("{0,-3:X}", hexNums);
            }

            string newHeader = builder.ToString();

            Size size = MeasureHeaderString(newHeader, HexHeaderTextBlock);

            if (size.Width <= width || header == null)
            {
                header = newHeader;
                hexTargetNums = newTarget;
            }

            if (size.Width > width)
                break;
        }

        return hexTargetNums;
    }

    private int FindAsciiCharsPerRow(double width)
    {
        int asciiChars = 0;

        string? header = null;

        StringBuilder builder = new();

        while (true)
        {
            int newTarget = asciiChars + 8;

            for (int hexNums = asciiChars; hexNums < newTarget; hexNums++)
            {
                builder.AppendFormat("{0:X}", hexNums % 16);
            }

            string newHeader = builder.ToString();

            Size size = MeasureHeaderString(newHeader, AsciiHeaderTextBlock);

            if (size.Width <= width || header == null)
            {
                header = newHeader;
                asciiChars = newTarget;
            }

            if (size.Width > width)
                break;
        }

        return asciiChars;
    }

    private Size MeasureHeaderString(string candidate, TextBlock tbHeader)
    {

        var formattedText = new FormattedText(
            candidate,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(tbHeader.FontFamily, tbHeader.FontStyle, tbHeader.FontWeight, tbHeader.FontStretch),
            tbHeader.FontSize,
            Brushes.Black,
            new NumberSubstitution(),
            1);

        return new Size(formattedText.Width, formattedText.Height);
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