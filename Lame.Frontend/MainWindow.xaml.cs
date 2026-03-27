using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Lame.Frontend.ViewModels;

namespace Lame.Frontend;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void DialogBackground_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        var grid = sender as Grid;
        var border = grid?.Children.OfType<Border>().FirstOrDefault();

        // Trigger the ViewModel command when the Grid is clicked outside the Border
        if (border != null && !border.IsMouseOver)
        {
            var viewModel = DataContext as MainWindowViewModel;
            viewModel?.CloseDialogCommand.Execute(null);
        }
    }
}