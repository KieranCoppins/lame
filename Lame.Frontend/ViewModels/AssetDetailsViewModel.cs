using System.Collections.ObjectModel;
using System.Windows.Input;
using Lame.Backend.Assets;
using Lame.Backend.Languages;
using Lame.Backend.Tags;
using Lame.Backend.Translations;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Factories;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Frontend.ViewModels;

public class AssetDetailsViewModel : AssetViewModel
{
    private readonly IAssets _assetsService;
    private readonly IDialogService _dialogService;
    private readonly ILanguages _languagesService;

    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;
    private readonly ITags _tagsService;
    private readonly ITranslations _translationsService;

    private LinkAssetsDialogViewModel? _linkAssetsDialogViewModel;

    public AssetDetailsViewModel(
        INavigationService navigationService,
        IServiceProvider serviceProvider,
        ITranslations translationsService,
        ITags tagsService,
        IAssets assetsService,
        IDialogService dialogService,
        INotificationService notificationService,
        ILanguages languagesService,
        LinkAssetsDialogViewModelFactory linkAssetsDialogViewModelFactory,
        AssetDto asset,
        int supportedLanguagesCount) : base(asset, supportedLanguagesCount)
    {
        _navigationService = navigationService;
        _translationsService = translationsService;
        _tagsService = tagsService;
        _assetsService = assetsService;
        _dialogService = dialogService;
        _notificationService = notificationService;
        _languagesService = languagesService;

        Translations = [];
        LinkedAssets = [];
        Tags = [];

        ReturnToLibraryCommand = new RelayCommand(() =>
            _navigationService.NavigateTo(serviceProvider.GetRequiredService<AssetLibraryViewModel>));

        ViewLinkedAssetDetails = new RelayCommand<AssetViewModel>(linkedAsset =>
            _navigationService.NavigateTo(() =>
                ActivatorUtilities.CreateInstance<AssetDetailsViewModel>(
                    serviceProvider,
                    linkedAsset.Asset,
                    SupportedLanguagesCount
                )));

        OpenLinkAssetDialogCommand = new RelayCommand(() =>
        {
            _linkAssetsDialogViewModel = linkAssetsDialogViewModelFactory.Create(this, LinkToAsset);
            _linkAssetsDialogViewModel.OnAssetLinked += OnNavigatedTo;
            _dialogService.ShowDialog(_linkAssetsDialogViewModel);
        });

        Page = AppPage.Library;
    }

    public ObservableCollection<TranslationViewModel> Translations { get; }
    public ObservableCollection<AssetViewModel> LinkedAssets { get; }
    public ObservableCollection<TagViewModel> Tags { get; }

    public ICommand ReturnToLibraryCommand { get; }
    public ICommand ViewLinkedAssetDetails { get; }
    public ICommand OpenLinkAssetDialogCommand { get; }

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();
        Task.WhenAll(LoadTranslations(), LoadLinkedAssets(), LoadTags());
    }

    private async Task LoadTranslations()
    {
        var translations = await _translationsService.GetForAsset(Asset.Id);

        Translations.Clear();
        foreach (var translation in translations) Translations.Add(new TranslationViewModel(translation));
    }

    private async Task LoadLinkedAssets()
    {
        var linkedAssets = await _assetsService.GetLinkedAssets(Asset.Id);
        LinkedAssets.Clear();
        foreach (var linkedAsset in linkedAssets)
            LinkedAssets.Add(new AssetViewModel(linkedAsset, SupportedLanguagesCount));
    }

    private async Task LoadTags()
    {
        var tags = await _tagsService.GetTagsForResource(Asset.Id);
        Tags.Clear();
        foreach (var tag in tags) Tags.Add(new TagViewModel(tag));
    }

    private async Task LinkToAsset(AssetDto asset)
    {
        await _assetsService.LinkAssets(Asset.Id, asset.Id);

        _notificationService.EmitNotification(new Notification
        {
            Message =
                $"Asset '{asset.InternalName}' successfully linked to '{Asset.InternalName}'",
            Type = NotificationType.Success,
            Title = "Assets linked"
        });
    }
}