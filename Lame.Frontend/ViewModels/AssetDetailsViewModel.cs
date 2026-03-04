using System.Collections.ObjectModel;
using System.Windows.Input;
using Lame.Backend.Assets;
using Lame.Backend.Tags;
using Lame.Backend.Translations;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Frontend.ViewModels;

public class AssetDetailsViewModel : AssetViewModel
{
    
    public ObservableCollection<TranslationViewModel> Translations { get; }
    public ObservableCollection<AssetViewModel> LinkedAssets { get; }
    public ObservableCollection<TagViewModel> Tags { get; }
    
    public ICommand ReturnToLibraryCommand { get; }
    public ICommand ViewLinkedAssetDetails { get; }
    
    private readonly INavigationService _navigationService;
    private readonly ITranslations _translationsService;
    private readonly ITags _tagsService;
    private readonly IAssets _assetsService;
    
    public AssetDetailsViewModel(
        INavigationService navigationService, 
        IServiceProvider serviceProvider, 
        ITranslations translationsService, 
        ITags tagsService,
        IAssets assetsService,
        AssetDto asset) : base(asset)
    {
        _navigationService = navigationService;
        _translationsService = translationsService;
        _tagsService = tagsService;
        _assetsService = assetsService;

        Translations = [];
        LinkedAssets = [];
        Tags = [];
        
        ReturnToLibraryCommand = new RelayCommand(() =>
            _navigationService.NavigateTo(serviceProvider.GetRequiredService<AssetLibraryViewModel>));
        
        ViewLinkedAssetDetails = new RelayCommand<AssetViewModel>(linkedAsset =>
            _navigationService.NavigateTo(() => ActivatorUtilities.CreateInstance<AssetDetailsViewModel>(serviceProvider, linkedAsset.Asset)));
        
        _navigationService.CurrentViewModelChanged += async () =>
        {
            if (_navigationService.CurrentViewModel == this)
            {
                Task.WaitAll([
                    LoadTranslations(),
                    LoadLinkedAssets(),
                    LoadTags()
                ]);
            }
        };

        Page = AppPage.Library;
    }

    private async Task LoadTranslations()
    {
        var translations = await _translationsService.GetForAsset(Asset.Id);
        Translations.Clear();
        foreach (var translation in translations)
        {
            Translations.Add(new TranslationViewModel(translation));
        }
    }

    private async Task LoadLinkedAssets()
    {
        var linkedAssets = await _assetsService.GetLinkedAssets(Asset.Id);
        LinkedAssets.Clear();
        foreach (var linkedAsset in linkedAssets)
        {
            LinkedAssets.Add(new AssetViewModel(linkedAsset));
        }
    }

    private async Task LoadTags()
    {
        var tags = await _tagsService.GetTagsForResource(Asset.Id);
        Tags.Clear();
        foreach (var tag in tags)
        {
            Tags.Add(new TagViewModel(tag));
        }
    }
}