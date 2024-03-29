﻿using Comqueror.Properties;
using Comqueror.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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

    private MessageLogViewModel? _viewModel;

    public MessageLogView()
    {
        InitializeComponent();

        //CbAutoScroll.IsChecked = Settings.Default.Log_AutoScroll;

        //if (DataContext is MessageLogViewModel viewModel)
        //    _viewModel = viewModel;
    }

    private void HexHeaderTextBlockLoaded(object sender, RoutedEventArgs e) => HexHeaderTextBlock = sender as TextBlock;

    private void AsciiHeaderTextBlockLoaded(object sender, RoutedEventArgs e) => AsciiHeaderTextBlock = sender as TextBlock;

    private void HexHeaderRectLoaded(object sender, RoutedEventArgs e) => HexHeaderRect = sender as Rectangle;

    private void AsciiHeaderRectLoaded(object sender, RoutedEventArgs e) => AsciiHeaderRect = sender as Rectangle;

    private void MessageHeaderSizeChanged(object sender, SizeChangedEventArgs e)
    {
        FindBytesPerRow();
    }

    /// <summary>
    /// Finds the number of bytes that can be displayed in both the ascii and the hex column.
    /// </summary>
    private void FindBytesPerRow()
    {
        if (_viewModel == null || HexHeaderTextBlock == null || AsciiHeaderTextBlock == null || HexHeaderRect == null || AsciiHeaderRect == null)
            return;

        int hexNumbersPerRow = FindHexNumsPerRow(HexHeaderRect.ActualWidth);

        int asciiCharsPerRow = FindAsciiCharsPerRow(AsciiHeaderRect.ActualWidth);

        int messageBytesPerRow = Math.Min(hexNumbersPerRow, asciiCharsPerRow);

        if (messageBytesPerRow != _viewModel.MessageBytesPerRow)
        {
            StringBuilder builder = new();

            for (int i = 0; i < messageBytesPerRow; i++)
            {
                builder.AppendFormat("{0,-3:X}", i);
            }

            HexHeaderTextBlock.Text = builder.ToString();

            builder.Clear();

            for (int i = 0; i < messageBytesPerRow; i++)
            {
                builder.AppendFormat("{0:X}", i % 16);
            }

            AsciiHeaderTextBlock.Text = builder.ToString();

            _viewModel.MessageBytesPerRow = messageBytesPerRow;
        }
    }

    /// <summary>
    /// Finds the maximum number of Hex-Numbers that fit into the Hex-Column.
    /// </summary>
    /// <param name="width">Width of the column.</param>
    /// <returns>The number of hex bytes that fits into the column.</returns>
    private int FindHexNumsPerRow(double width)
    {
        Debug.Assert(HexHeaderTextBlock != null);

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

    /// <summary>
    /// Finds the maximum number of Ascii-chars that fit into the Ascii-Column.
    /// </summary>
    /// <param name="width">Width of the column.</param>
    /// <returns>The number of ascii chars that fits into the column.</returns>
    private int FindAsciiCharsPerRow(double width)
    {
        Debug.Assert(AsciiHeaderTextBlock != null);

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

    private static Size MeasureHeaderString(string candidate, TextBlock tbHeader)
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

    private static int StartOfLine(string text, int index)
    {
        while (true)
        {
            if (index <= 0)
                return 0;

            if (text[index] == '\n')
                return index + 1;

            index--;
        }
    }

    private void TxtBla(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;

        //Debug.WriteLine($"{textBox.SelectionStart} | {textBox.SelectionLength}");

        //textBox.Selection

        // Fix start of selection
        {
            int startOfLine = StartOfLine(textBox.Text, textBox.SelectionStart);

            int indexInLine = textBox.SelectionStart - startOfLine;

            int selectionStart = startOfLine + (indexInLine / 3 * 3);

            int difference = selectionStart - textBox.SelectionStart;

            if (difference != 0)
            {
                textBox.SelectionStart = selectionStart;
                //textBox.SelectionLength += difference;
            }
        }

        // Fix selection length

        //int mod = textBox.SelectionLength % 3;

        //if (mod != 0)
        //{
        //    textBox.SelectionLength += (3 - mod);
        //}
    }

    private void UC_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (DataContext is MessageLogViewModel viewModel)
            _viewModel = viewModel;
    }
}