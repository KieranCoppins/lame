namespace Lame.Frontend.Services;

public interface IDialogService
{
    public bool IsDialogOpen { get; }
    public object ActiveDialog { get; }

    void ShowDialog(object viewModel);

    void CloseDialog();

    public event Action ActiveDialogChanged;
}