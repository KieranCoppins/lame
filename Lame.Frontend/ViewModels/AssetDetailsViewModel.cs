using System.Collections.ObjectModel;
using System.Windows.Input;
using Lame.Backend.Translations;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Frontend.ViewModels;

public class AssetDetailsViewModel : AssetViewModel
{
    
    public ObservableCollection<TranslationViewModel> Translations { get; }
    
    public ICommand ReturnToLibraryCommand { get; }
    private readonly INavigationService _navigationService;
    private readonly ITranslations _translationsService;
    
    public AssetDetailsViewModel(
        INavigationService navigationService, 
        IServiceProvider serviceProvider, 
        ITranslations translationsService, 
        AssetDto asset) : base(asset)
    {
        _navigationService = navigationService;
        _translationsService = translationsService;

        Translations = [];
        
        ReturnToLibraryCommand = new RelayCommand(() =>
            _navigationService.NavigateTo(serviceProvider.GetRequiredService<AssetLibraryViewModel>));
        
        _navigationService.CurrentViewModelChanged += async () =>
        {
            if (_navigationService.CurrentViewModel == this)
            {
                await LoadTranslations();
            }
        };
    }

    private async Task LoadTranslations()
    {
        // var translations = await _translationsService.GetForAsset(Asset.Id);
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
}