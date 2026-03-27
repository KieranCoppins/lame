using System;
using System.Windows.Input;

namespace Lame.Frontend.Commands;

public abstract class BaseRelayCommand<T> : ICommand
{
    private readonly Func<T, bool>? _canExecute;

    public BaseRelayCommand(Func<T, bool>? canExecute = null)
    {
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(GetParameterValue(parameter)) ?? true;
    }

    public virtual void Execute(object? parameter)
    {
        throw new NotImplementedException();
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    protected static T GetParameterValue(object? parameter)
    {
        if (parameter == null) return default;

        return (T)parameter;
    }
}