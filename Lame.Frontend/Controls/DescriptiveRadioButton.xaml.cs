using System.Windows;
using System.Windows.Controls;

namespace Lame.Frontend.Controls;

public partial class DescriptiveRadioButton : RadioButton
{
    public static readonly DependencyProperty BodyTextProperty =
        DependencyProperty.Register(
            nameof(BodyText),
            typeof(string),
            typeof(DescriptiveRadioButton),
            new PropertyMetadata(null, OnBodyTextChanged));

    public static readonly DependencyProperty TitleTextProperty =
        DependencyProperty.Register(
            nameof(TitleText),
            typeof(string),
            typeof(DescriptiveRadioButton),
            new PropertyMetadata(null, OnTitleTextChanged));

    public DescriptiveRadioButton()
    {
        InitializeComponent();
    }

    public string BodyText
    {
        get => (string)GetValue(BodyTextProperty);
        set => SetValue(BodyTextProperty, value);
    }

    public string TitleText
    {
        get => (string)GetValue(TitleTextProperty);
        set => SetValue(TitleTextProperty, value);
    }

    private static void OnBodyTextChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        var control = (DescriptiveRadioButton)d;
        control.BodyText = (string)e.NewValue;
    }

    private static void OnTitleTextChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        var control = (DescriptiveRadioButton)d;
        control.TitleText = (string)e.NewValue;
    }
}