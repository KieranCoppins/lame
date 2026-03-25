using System.Collections.ObjectModel;
using System.Windows.Input;
using Lame.Backend.ChangeLog;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Enums;
using Lame.Frontend.Models;
using Lame.Frontend.Services;

namespace Lame.Frontend.ViewModels;

public class ChangeLogViewModel : PageViewModel
{
    private readonly IChangeLog _changeLogService;
    private readonly INotificationService _notificationService;

    public ChangeLogViewModel(IChangeLog changeLogService, INotificationService notificationService)
    {
        _changeLogService = changeLogService;
        _notificationService = notificationService;

        Page = AppPage.ChangeLog;

        Entries = [];
        PageNumbers = [];
        CurrentPage = 0;

        SetPageCommand = new RelayCommand<int>(page =>
        {
            CurrentPage = page;
            _ = LoadEntries();
        });
    }

    public ObservableCollection<ChangeLogEntry> Entries { get; }
    public ObservableCollection<PageNumber> PageNumbers { get; }

    public int CurrentPage { get; set; }

    public ICommand SetPageCommand { get; }

    public override async Task OnNavigatedTo()
    {
        await base.OnNavigatedTo();
        await LoadEntries();
    }

    private async Task LoadEntries()
    {
        try
        {
            var entries = await _changeLogService.Get(CurrentPage, 25);
            Entries.Clear();
            foreach (var entry in entries.Items) Entries.Add(entry);

            PageNumbers.Clear();
            for (var i = 0; i < entries.TotalPages; i++) PageNumbers.Add(new PageNumber { Number = i });
        }
        catch (Exception ex)
        {
            _notificationService.EmitNotification(new Notification
            {
                Title = "Error getting change log",
                Message = $"An error occured getting the change log: {ex.Message}",
                Type = NotificationType.Failure
            });
        }
    }
}