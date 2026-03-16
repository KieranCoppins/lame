using Lame.Frontend.Commands;

namespace Lame.Frontend.Tests.CommandTests;

public class RelayCommandTests
{
    [Fact]
    public void Execute_ShouldInvokeAction_WithCorrectParameter()
    {
        // Arrange
        object? received = null;
        var command = new RelayCommand<object>(x => received = x);

        // Act
        command.Execute("test");

        // Assert
        Assert.Equal("test", received);
    }

    [Fact]
    public void Execute_ShouldInvokeAction_WhenCanExecuteIsNull()
    {
        // Arrange
        var executed = false;
        var command = new RelayCommand<object>(_ => executed = true);

        // Act
        command.Execute(42);

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public void Execute_ShouldNotThrow_WhenParameterIsNull()
    {
        // Arrange
        var command = new RelayCommand<object>(_ => { });

        // Act
        var exception = Record.Exception(() => command.Execute(null));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void RelayCommand_NonGeneric_Execute_ShouldInvokeAction()
    {
        // Arrange
        var executed = false;
        var command = new RelayCommand(() => executed = true);

        // Act
        command.Execute(null);

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public void RelayCommand_NonGeneric_CanExecuteFalse_ShouldNotInvokeAction()
    {
        // Arrange
        var executed = false;
        var command = new RelayCommand(() => executed = true, () => false);

        // Act
        if (command.CanExecute(null))
            command.Execute(null);

        // Assert
        Assert.False(executed);
    }

    [Fact]
    public void RelayCommand_Generic_CanExecuteFalse_ShouldNotInvokeAction()
    {
        // Arrange
        var executed = false;
        var command = new RelayCommand<object>(_ => executed = true, _ => false);

        // Act
        if (command.CanExecute("param"))
            command.Execute("param");

        // Assert
        Assert.False(executed);
    }
}