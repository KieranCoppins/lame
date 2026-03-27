using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Lame.Backend.Assets;
using Lame.Backend.Languages;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Models;
using Lame.Frontend.Services;

namespace Lame.Frontend.ViewModels;

public class AssetLibraryViewModel : PageViewModel
{
    private readonly IAssets _assets;
    private readonly ILanguages _languagesService;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;

    public AssetLibraryViewModel(
        IAssets assets,
        INavigationService navigationService,
        ILanguages languagesService,
        INotificationService notificationService,
        string? searchQuery = null)
    {
        _assets = assets;
        _navigationService = navigationService;
        _languagesService = languagesService;
        _notificationService = notificationService;

        Assets = [];
        PageNumbers = [];
        CurrentPage = 0;

        SetPageCommand = new AsyncRelayCommand<int>(async page =>
        {
            CurrentPage = page;
            await LoadAssets();
        });

        ViewAssetDetailsCommand = new RelayCommand<AssetDto>(OnViewAssetDetails);
        Page = AppPage.Library;

        if (searchQuery != null)
            SearchQuery = searchQuery;
    }

    public Task? SearchQueryTask { get; private set; }

    public ObservableCollection<PageNumber> PageNumbers { get; }

    public ICommand SetPageCommand { get; }

    public int CurrentPage
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsLoading
    {
        get;
        private set => SetField(ref field, value);
    }

    public string SearchQuery
    {
        get;
        set
        {
            SetField(ref field, value);
            SearchQueryTask = LoadAssets();
        }
    }

    public ObservableCollection<AssetDto> Assets { get; }

    public ICommand ViewAssetDetailsCommand { get; }

    public int SupportedLanguagesCount
    {
        get;
        private set => SetField(ref field, value);
    }

    public override async Task OnNavigatedTo()
    {
        await base.OnNavigatedTo();

        await Task.WhenAll(LoadAssets(), GetSupportedLanguagesCount());
    }

    private async Task GetSupportedLanguagesCount()
    {
        try
        {
            var languages = await _languagesService.Get();
            SupportedLanguagesCount = languages.Count;
        }
        catch (Exception ex)
        {
            _notificationService.EmitNotification(
                new Notification
                {
                    Title = "Error",
                    Message = ex.Message,
                    Type = NotificationType.Failure
                }
            );
        }
    }

    private async Task LoadAssets()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            List<AssetDto> assets;
            PageNumbers.Clear();
            if (string.IsNullOrEmpty(SearchQuery))
            {
                var response = await _assets.Get(CurrentPage, 25);
                assets = response.Items;

                for (var i = 0; i < response.TotalPages; i++) PageNumbers.Add(new PageNumber { Number = i });
            }
            else
            {
                assets = await _assets.Get(SearchQuery);
            }

            Assets.Clear();
            foreach (var asset in assets) Assets.Add(asset);
        }
        catch (Exception ex)
        {
            _notificationService.EmitNotification(
                new Notification
                {
                    Title = "Error",
                    Message = ex.Message,
                    Type = NotificationType.Failure
                }
            );
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnViewAssetDetails(AssetDto asset)
    {
        _navigationService.NavigateTo<AssetLibraryDetailsViewModel>(asset);
    }
}