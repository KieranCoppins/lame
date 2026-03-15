using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Lame.Frontend.Controls;

public partial class ConfirmationDialogControl : UserControl
{
    // Title as string
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(ConfirmationDialogControl),
            new PropertyMetadata(null));

    // Title as template
    public static readonly DependencyProperty TitleTemplateProperty =
        DependencyProperty.Register(
            nameof(TitleTemplate),
            typeof(DataTemplate),
            typeof(ConfirmationDialogControl),
            new PropertyMetadata(null));

    // Body as string
    public static readonly DependencyProperty BodyProperty =
        DependencyProperty.Register(
            nameof(Body),
            typeof(string),
            typeof(ConfirmationDialogControl),
            new PropertyMetadata(null));

    // Body template
    public static readonly DependencyProperty BodyTemplateProperty =
        DependencyProperty.Register(
            nameof(BodyTemplate),
            typeof(DataTemplate),
            typeof(ConfirmationDialogControl),
            new PropertyMetadata(null));

    // Button labels
    public static readonly DependencyProperty PrimaryButtonLabelProperty =
        DependencyProperty.Register(
            nameof(PrimaryButtonLabel),
            typeof(string),
            typeof(ConfirmationDialogControl),
            new PropertyMetadata("Confirm"));

    public static readonly DependencyProperty SecondaryButtonLabelProperty =
        DependencyProperty.Register(
            nameof(SecondaryButtonLabel),
            typeof(string),
            typeof(ConfirmationDialogControl),
            new PropertyMetadata("Cancel"));

    // Button commands
    public static readonly DependencyProperty PrimaryCommandProperty =
        DependencyProperty.Register(
            nameof(PrimaryCommand),
            typeof(ICommand),
            typeof(ConfirmationDialogControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SecondaryCommandProperty =
        DependencyProperty.Register(
            nameof(SecondaryCommand),
            typeof(ICommand),
            typeof(ConfirmationDialogControl),
            new PropertyMetadata(null));

    // Colours
    public static readonly DependencyProperty PrimaryButtonColourProperty =
        DependencyProperty.Register(
            nameof(PrimaryButtonColour),
            typeof(Brush),
            typeof(ConfirmationDialogControl),
            new PropertyMetadata(
                Application.Current.TryFindResource("Brush.Primary") as Brush ?? Brushes.DodgerBlue
            ));

    public static readonly DependencyProperty PrimaryButtonHoverColourProperty =
        DependencyProperty.Register(
            nameof(PrimaryButtonHoverColour),
            typeof(Brush),
            typeof(ConfirmationDialogControl),
            new PropertyMetadata(
                Application.Current.TryFindResource("Brush.Primary") as Brush ?? Brushes.DodgerBlue
            ));

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(ConfirmationDialogControl),
            new PropertyMetadata(false));

    public ConfirmationDialogControl()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Body
    {
        get => (string)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    public Brush PrimaryButtonColour
    {
        get => (Brush)GetValue(PrimaryButtonColourProperty);
        set => SetValue(PrimaryButtonColourProperty, value);
    }

    public Brush PrimaryButtonHoverColour
    {
        get => (Brush)GetValue(PrimaryButtonHoverColourProperty);
        set => SetValue(PrimaryButtonHoverColourProperty, value);
    }

    public DataTemplate TitleTemplate
    {
        get => (DataTemplate)GetValue(TitleTemplateProperty);
        set => SetValue(TitleTemplateProperty, value);
    }

    public DataTemplate BodyTemplate
    {
        get => (DataTemplate)GetValue(BodyTemplateProperty);
        set => SetValue(BodyTemplateProperty, value);
    }

    public string PrimaryButtonLabel
    {
        get => (string)GetValue(PrimaryButtonLabelProperty);
        set => SetValue(PrimaryButtonLabelProperty, value);
    }

    public string SecondaryButtonLabel
    {
        get => (string)GetValue(SecondaryButtonLabelProperty);
        set => SetValue(SecondaryButtonLabelProperty, value);
    }

    public ICommand PrimaryCommand
    {
        get => (ICommand)GetValue(PrimaryCommandProperty);
        set => SetValue(PrimaryCommandProperty, value);
    }

    public ICommand SecondaryCommand
    {
        get => (ICommand)GetValue(SecondaryCommandProperty);
        set => SetValue(SecondaryCommandProperty, value);
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
}