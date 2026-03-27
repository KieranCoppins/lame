using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Lame.Frontend.Controls;

public partial class FileUploaderControl : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty FilePathProperty =
        DependencyProperty.Register(
            nameof(FilePath),
            typeof(string),
            typeof(FileUploaderControl),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFilePathChanged));

    public static readonly DependencyProperty FileFilterProperty =
        DependencyProperty.Register(
            nameof(FileFilter),
            typeof(string),
            typeof(FileUploaderControl),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFileFilterChanged));

    public FileUploaderControl()
    {
        InitializeComponent();
    }

    public string FilePath
    {
        get => (string)GetValue(FilePathProperty);
        set => SetValue(FilePathProperty, value);
    }

    public string FileFilter
    {
        get => (string)GetValue(FileFilterProperty);
        set => SetValue(FileFilterProperty, value);
    }

    // Take the Windows file filter and convert it a comma separated list of file extensions
    public string ReadableFileFilter => string.Join(", ",
        FileFilter.Split('|')
            .Where((_, index) => index % 2 == 1)
            .SelectMany(filterPart => filterPart.Split(';'))
            .Select(ext => ext.TrimStart('*'))
            .Where(ext => !string.IsNullOrWhiteSpace(ext))
    );

    public string BodyText =>
        string.IsNullOrWhiteSpace(FilePath) ? "Drop files here or click to browse" : FilePath;

    public event PropertyChangedEventHandler? PropertyChanged;

    private static void OnFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (FileUploaderControl)d;
        control.OnPropertyChanged(nameof(BodyText));
    }

    private static void OnFileFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (FileUploaderControl)d;
        control.OnPropertyChanged(nameof(ReadableFileFilter));
    }

    private void FileDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0) SetCurrentValue(FilePathProperty, files[0]);
        }
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = FileFilter
        };

        if (dialog.ShowDialog() == true) SetCurrentValue(FilePathProperty, dialog.FileName);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}