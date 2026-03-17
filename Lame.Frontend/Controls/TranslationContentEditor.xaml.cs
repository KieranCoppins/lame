using System.Windows;
using System.Windows.Controls;
using Lame.DomainModel;

namespace Lame.Frontend.Controls;

public partial class TranslationContentEditor : UserControl
{
    public static readonly DependencyProperty AssetTypeProperty =
        DependencyProperty.Register(
            nameof(AssetType),
            typeof(AssetType),
            typeof(TranslationContentEditor),
            new PropertyMetadata(AssetType.Text));

    public static readonly DependencyProperty TranslationContentProperty =
        DependencyProperty.Register(
            nameof(TranslationContent),
            typeof(string),
            typeof(TranslationContentEditor),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public TranslationContentEditor()
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
}