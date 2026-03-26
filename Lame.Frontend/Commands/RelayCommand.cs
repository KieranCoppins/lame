using System;

namespace Lame.Frontend.Commands;

public class RelayCommand<T> : BaseRelayCommand<T>
{
    private readonly Action<T> _execute;

    public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null) : base(canExecute)
    {
        _execute = execute;
    }

    public override void Execute(object? parameter)
    {
        _execute(GetParameterValue(parameter));
    }
}

public class RelayCommand : RelayCommand<object>
{
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
        : base(_ => execute(), canExecute == null ? null : _ => canExecute())
    {
    }
}