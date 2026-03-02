using System.Windows.Controls;
using Lame.DomainModel;

namespace Lame.Frontend.Views;

public partial class CreateAssetView : UserControl
{
    public CreateAssetView()
    {
        InitializeComponent();
        
        AssetTypeComboBox.ItemsSource = Enum.GetNames(typeof(AssetType));
        AssetTypeComboBox.SelectedItem = nameof(AssetType.Text);
    }
}