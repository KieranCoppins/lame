using System;

namespace Lame.Frontend.Services;

public interface IDialogService
{
    public bool IsDialogOpen { get; }
    public object? ActiveDialog { get; }

    void ShowDialog(object viewModel);

    void ShowDialog<T>(params object[] args);

    void CloseDialog();

    public event Action ActiveDialogChanged;
}