using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Lame.DomainModel;

namespace Lame.Frontend.Controls;

public partial class TranslationContentDisplay : UserControl
{
    public static readonly DependencyProperty AssetTypeProperty =
        DependencyProperty.Register(
            nameof(AssetType),
            typeof(AssetType),
            typeof(TranslationContentDisplay),
            new PropertyMetadata(AssetType.Text));

    public static readonly DependencyProperty TranslationContentProperty =
        DependencyProperty.Register(
            nameof(TranslationContent),
            typeof(string),
            typeof(TranslationContentDisplay),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty DownloadCommandProperty =
        DependencyProperty.Register(
            nameof(DownloadCommand),
            typeof(ICommand),
            typeof(TranslationContentDisplay),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DownloadCommandParameterProperty =
        DependencyProperty.Register(
            nameof(DownloadCommandParameter),
            typeof(object),
            typeof(TranslationContentDisplay),
            new PropertyMetadata(null));

    public TranslationContentDisplay()
    {
        InitializeComponent();
    }

    public AssetType AssetType
    {
        get => (AssetType)GetValue(AssetTypeProperty);
        set => SetValue(AssetTypeProperty, value);
    }

    public string TranslationContent
    {
        get => (string)GetValue(TranslationContentProperty);
        set => SetValue(TranslationContentProperty, value);
    }

    public ICommand DownloadCommand
    {
        get => (ICommand)GetValue(DownloadCommandProperty);
        set => SetValue(DownloadCommandProperty, value);
    }

    public object? DownloadCommandParameter
    {
        get => GetValue(DownloadCommandParameterProperty);
        set => SetValue(DownloadCommandParameterProperty, value);
    }
}