using System;
using System.Threading.Tasks;

namespace Lame.Frontend.Commands;

public class AsyncRelayCommand<T> : BaseRelayCommand<T>
{
    private readonly Func<T, Task> _execute;

    public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool>? canExecute = null) : base(canExecute)
    {
        _execute = execute;
    }

    public Task? CommandTask { get; private set; }

    public override void Execute(object? parameter)
    {
        CommandTask = _execute(GetParameterValue(parameter));
    }
}

public class AsyncRelayCommand : AsyncRelayCommand<object>
{
    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        : base(_ => execute(), canExecute == null ? null : _ => canExecute())

    {
    }
}