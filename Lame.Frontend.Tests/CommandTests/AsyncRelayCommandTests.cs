using Lame.Frontend.Commands;

namespace Lame.Frontend.Tests.CommandTests;

public class AsyncRelayCommandTests
{
    [Fact]
    public async Task Execute_ShouldInvokeAsyncAction_WithCorrectParameter()
    {
        // Arrange
        object? received = null;
        var command = new AsyncRelayCommand<object>(async x =>
        {
            received = x;
            await Task.CompletedTask;
        });

        // Act
        command.Execute("asyncTest");
        await command.CommandTask!;

        // Assert
        Assert.Equal("asyncTest", received);
    }

    [Fact]
    public async Task Execute_ShouldSetCommandTask_WhenInvoked()
    {
        // Arrange
        var command = new AsyncRelayCommand<object>(async _ => await Task.Delay(10));

        // Act
        command.Execute(null);

        // Assert
        Assert.NotNull(command.CommandTask);
        await command.CommandTask!;
    }

    [Fact]
    public async Task Execute_ShouldNotThrow_WhenParameterIsNull()
    {
        // Arrange
        var command = new AsyncRelayCommand<object>(async _ => await Task.CompletedTask);

        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            command.Execute(null);
            await command.CommandTask!;
        });

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task AsyncRelayCommand_NonGeneric_Execute_ShouldInvokeAsyncAction()
    {
        // Arrange
        var executed = false;
        var command = new AsyncRelayCommand(async () =>
        {
            executed = true;
            await Task.CompletedTask;
        });

        // Act
        command.Execute(null);
        await command.CommandTask!;

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public void AsyncRelayCommand_Generic_CanExecuteFalse_ShouldNotInvokeAction()
    {
        // Arrange
        var executed = false;
        var command = new AsyncRelayCommand<object>(async _ =>
        {
            executed = true;
            await Task.CompletedTask;
        }, _ => false);

        // Act
        if (command.CanExecute("param"))
            command.Execute("param");

        // Assert
        Assert.False(executed);
    }

    [Fact]
    public void AsyncRelayCommand_NonGeneric_CanExecuteFalse_ShouldNotInvokeAction()
    {
        // Arrange
        var executed = false;
        var command = new AsyncRelayCommand(async () =>
        {
            executed = true;
            await Task.CompletedTask;
        }, () => false);

        // Act
        if (command.CanExecute(null))
            command.Execute(null);

        // Assert
        Assert.False(executed);
    }
}