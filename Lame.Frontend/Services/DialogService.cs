using Microsoft.Extensions.DependencyInjection;

namespace Lame.Frontend.Services;

public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool IsDialogOpen { get; private set; }
    public object? ActiveDialog { get; private set; }

    public void ShowDialog(object viewModel)
    {
        ActiveDialog = viewModel;
        IsDialogOpen = true;
        ActiveDialogChanged?.Invoke();
    }

    public void ShowDialog<T>(params object[] args)
    {
        ActiveDialog = ActivatorUtilities.CreateInstance<T>(_serviceProvider, args);
        IsDialogOpen = true;
        ActiveDialogChanged?.Invoke();
    }

    public void CloseDialog()
    {
        ActiveDialog = null;
        IsDialogOpen = false;
        ActiveDialogChanged?.Invoke();
    }


    public event Action ActiveDialogChanged;
}