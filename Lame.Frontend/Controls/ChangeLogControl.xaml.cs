using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Lame.DomainModel;

namespace Lame.Frontend.Controls;

public partial class ChangeLogControl : UserControl
{
    public static readonly DependencyProperty EntriesProperty =
        DependencyProperty.Register(
            nameof(Entries),
            typeof(ObservableCollection<ChangeLogEntry>),
            typeof(ChangeLogControl),
            new PropertyMetadata(new ObservableCollection<ChangeLogEntry>()));

    public ChangeLogControl()
    {
        InitializeComponent();
    }

    public ObservableCollection<ChangeLogEntry> Entries
    {
        get => (ObservableCollection<ChangeLogEntry>)GetValue(EntriesProperty);
        set => SetValue(EntriesProperty, value);
    }
}