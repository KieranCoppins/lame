using System.Collections.ObjectModel;
using System.Windows.Input;
using Lame.Backend.Assets;
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
    
    public ICommand ReturnToLibraryCommand { get; }
    public ICommand ViewLinkedAssetDetails { get; }
    
    private readonly INavigationService _navigationService;
    private readonly ITranslations _translationsService;
    private readonly IAssets _assetsService;
    
    public AssetDetailsViewModel(
        INavigationService navigationService, 
        IServiceProvider serviceProvider, 
        ITranslations translationsService, 
        IAssets assetsService,
        AssetDto asset) : base(asset)
    {
        _navigationService = navigationService;
        _translationsService = translationsService;
        _assetsService = assetsService;

        Translations = [];
        LinkedAssets = [];
        
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
                    LoadLinkedAssets()
                ]);
            }
        };

        Page = AppPage.Library;
    }

    private async Task LoadTranslations()
    {
        // var translations = await _translationsService.GetForAsset(Asset.Id);
        
        // Dummy data for testing
        List<Translation> translations =
        [
            new()
            {
                Id = Guid.NewGuid(), AssetId = Asset.Id, Language = "en",
                Content =
                    "Welcome, brave adventurer! The kingdom needs your help. An ancient artifact has been stolen from the royal vault, and dark forces are gathering at the northern border.",
                MajorVersion = 1, MinorVersion = 0,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(), AssetId = Asset.Id, Language = "es",
                Content =
                    "¡Bienvenido, valiente aventurero! El reino necesita tu ayuda. Un artefacto antiguo ha sido robado de la bóveda real...",
                MajorVersion = 1, MinorVersion = 3,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(), AssetId = Asset.Id, Language = "fr",
                Content =
                    "Bienvenue, brave aventurier ! Le royaume a besoin de votre aide. Un artefact ancien a été volé...",
                MajorVersion = 1, MinorVersion = 8,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = Guid.NewGuid(), AssetId = Asset.Id, Language = "de",
                Content =
                    "Willkommen, mutiger Abenteurer! Das Königreich braucht deine Hilfe. Ein uraltes Artefakt wurde...",
                MajorVersion = 1, MinorVersion = 23,
                CreatedAt = DateTime.Now
            }
        ];
        
        Translations.Clear();
        foreach (var translation in translations)
        {
            Translations.Add(new TranslationViewModel(translation));
        }
    }

    private async Task LoadLinkedAssets()
    {
        // var linkedAssets = await _assetsService.GetLinkedAssets(Asset.Id);
        
        // Dummy data for testing
        List<AssetDto> linkedAssets =
        [
            new() { Id = Guid.NewGuid(), InternalName = "ui_main_menu_title", AssetType = DomainModel.AssetType.Text },
            new() { Id = Guid.NewGuid(), InternalName = "dialog_quest_01_intro", AssetType = DomainModel.AssetType.Text },
            new() { Id = Guid.NewGuid(), InternalName = "voice_quest_01_intro", AssetType = DomainModel.AssetType.Audio },
        ];
        
        LinkedAssets.Clear();
        foreach (var linkedAsset in linkedAssets)
        {
            LinkedAssets.Add(new AssetViewModel(linkedAsset));
        }
    }
}