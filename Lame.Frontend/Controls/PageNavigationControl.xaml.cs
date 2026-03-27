using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Lame.Frontend.Models;

namespace Lame.Frontend.Controls;

public partial class PageNavigationControl : UserControl
{
    public static readonly DependencyProperty PageCommandProperty =
        DependencyProperty.Register(
            nameof(PageCommand),
            typeof(ICommand),
            typeof(PageNavigationControl));

    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register(
            nameof(CurrentPage),
            typeof(int),
            typeof(PageNavigationControl));

    public static readonly DependencyProperty PageNumbersProperty =
        DependencyProperty.Register(
            nameof(PageNumbers),
            typeof(ObservableCollection<PageNumber>),
            typeof(PageNavigationControl),
            new PropertyMetadata(new ObservableCollection<PageNumber>()));

    public PageNavigationControl()
    {
        InitializeComponent();
    }

    public ICommand PageCommand
    {
        get => (ICommand)GetValue(PageCommandProperty);
        set => SetValue(PageCommandProperty, value);
    }

    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public ObservableCollection<PageNumber> PageNumbers
    {
        get => (ObservableCollection<PageNumber>)GetValue(PageNumbersProperty);
        set => SetValue(PageNumbersProperty, value);
    }
}