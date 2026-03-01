using System.Windows.Input;

namespace Lame.Frontend.Commands;

public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool>? _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        T value = GetParameterValue(parameter);
        return _canExecute == null || _canExecute(value);
    }

    public void Execute(object? parameter)
    {
        T value = GetParameterValue(parameter);
        _execute(value);
    }

    private static T GetParameterValue(object? parameter)
    {
        if (parameter == null)
        {
            return default;
        }
        
        return (T)parameter;
    }

    public event EventHandler? CanExecuteChanged;
}

public class RelayCommand : RelayCommand<object>
{
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
        : base(_ => execute(), canExecute == null ? null : _ => canExecute())
    {
    }
}