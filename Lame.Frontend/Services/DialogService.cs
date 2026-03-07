namespace Lame.Frontend.Services;

public class DialogService : IDialogService
{
    public bool IsDialogOpen { get; private set; }
    public object ActiveDialog { get; private set; }

    public void ShowDialog(object viewModel)
    {
        ActiveDialog = viewModel;
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