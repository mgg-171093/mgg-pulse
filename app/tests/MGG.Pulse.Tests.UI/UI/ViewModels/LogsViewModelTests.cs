using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Infrastructure.Logging;
using MGG.Pulse.UI.ViewModels;
using Xunit;

namespace MGG.Pulse.Tests.UI.ViewModels;

public class LogsViewModelTests
{
    [Fact]
    public void LogText_WhenLoggerPublishesEntries_AccumulatesTranscript()
    {
        var logger = new FileLoggerService();
        var viewModel = new LogsViewModel(logger);

        logger.Log(LogLevel.Normal, "first message");
        logger.Log(LogLevel.Verbose, "second message");

        Assert.Contains("first message", viewModel.LogText);
        Assert.Contains("second message", viewModel.LogText);
    }

    [Fact]
    public async Task LogText_WhenNavigatingAwayAndReturning_KeepsSessionContext()
    {
        var logger = new FileLoggerService();
        var viewModel = new LogsViewModel(logger);

        logger.Log(LogLevel.Normal, "before away");
        var snapshotBeforeNavigation = viewModel.LogText;

        // Simulate in-session re-navigation to the same singleton ViewModel instance.
        var snapshotAfterReturn = viewModel.LogText;

        await logger.LogAsync(LogLevel.Verbose, "after return");

        Assert.Equal(snapshotBeforeNavigation, snapshotAfterReturn);
        Assert.Contains("before away", viewModel.LogText);
        Assert.Contains("after return", viewModel.LogText);
    }
}
