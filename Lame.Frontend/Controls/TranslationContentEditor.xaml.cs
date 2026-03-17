using System.Windows;
using System.Windows.Controls;
using Lame.DomainModel;
using Microsoft.Win32;

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

    private void BrowseAudio_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Audio Files|*.mp3;*.wav;*.ogg;*.flac;*.m4a;|All Files|*.*"
        };

        if (dialog.ShowDialog() == true) TranslationContent = dialog.FileName;
    }

    private void AudioDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0) TranslationContent = files[0];
        }
    }
}