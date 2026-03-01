using System.ComponentModel;
using System.Windows.Input;
using Lame.DomainModel;

namespace Lame.Frontend.ViewModels;

public class AssetViewModel : INotifyPropertyChanged
{
    protected readonly Asset _asset;
    
    public string InternalName => _asset.InternalName;
    
    public string ContentType => _asset.AssetType.ToString();
    
    public ICommand ViewDetailsCommand { get; }

    public AssetViewModel(Asset asset)
    {
        _asset = asset;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}