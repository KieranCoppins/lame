using System.ComponentModel;
using System.Windows.Input;
using Lame.DomainModel;

namespace Lame.Frontend.ViewModels;

public class AssetViewModel : INotifyPropertyChanged
{
    protected readonly AssetDto _asset;
    
    public string InternalName => _asset.InternalName;
    
    public string AssetType => _asset.AssetType.ToString();
    
    // TODO set the total number of supported translations somewhere globally through some config
    public string Progress => $"{_asset.NumTranslations} / 15 ";

    public string LastModified => _asset.LastUpdatedAt.ToString("yyyy-mm-dd");
    
    public ICommand ViewDetailsCommand { get; }

    public AssetViewModel(AssetDto asset)
    {
        _asset = asset;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}