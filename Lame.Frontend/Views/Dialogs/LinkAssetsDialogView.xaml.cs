using System.Windows;
using System.Windows.Controls;

namespace Lame.Frontend.Views.Dialogs;

public partial class LinkAssetsDialogView : UserControl
{
    public LinkAssetsDialogView()
    {
        InitializeComponent();
    }

    private void AssetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var comboBox = (ComboBox)sender;

        if (comboBox.SelectedItem == null) comboBox.IsDropDownOpen = true;

        if (comboBox.Template.FindName("PART_EditableTextBox", comboBox) is TextBox textBox)
        {
            textBox.CaretIndex = textBox.Text.Length;
            textBox.ScrollToEnd();
        }
    }

    private void AssetComboBox_OnIsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var comboBox = (ComboBox)sender;

        comboBox.IsDropDownOpen = comboBox.IsKeyboardFocusWithin || comboBox.IsKeyboardFocused;
    }
}