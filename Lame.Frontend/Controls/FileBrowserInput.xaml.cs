using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;

namespace Lame.Frontend.Controls;

public partial class FileBrowserInput : UserControl
{
    public static readonly DependencyProperty FilePathProperty =
        DependencyProperty.Register(
            nameof(FilePath),
            typeof(string),
            typeof(FileBrowserInput),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public FileBrowserInput()
    {
        InitializeComponent();
    }

    public string FilePath
    {
        get => (string)GetValue(FilePathProperty);
        set => SetValue(FilePathProperty, value);
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        using (var dialog = new WinForms.FolderBrowserDialog())
        {
            dialog.SelectedPath = FilePath;
            if (dialog.ShowDialog() == WinForms.DialogResult.OK) FilePath = dialog.SelectedPath;
        }
    }
}