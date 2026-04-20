using MGG.Pulse.Application.Common;
using MGG.Pulse.Application.Updates;
using MGG.Pulse.Domain.Ports;
using MGG.Pulse.Domain.Updates;
using MGG.Pulse.UI.Updates;
using MGG.Pulse.UI.ViewModels;
using Microsoft.UI.Xaml;
using Moq;
using Xunit;

namespace MGG.Pulse.Tests.UI.UI.Updates;

public class UpdaterPromptFlowTests
{
    [Fact]
    public async Task StartupAvailableUpdate_AsksForConfirmationBeforeApplying()
    {
        var promptCalls = 0;
        var applyCalls = 0;

        var coordinator = new UpdateApplyCoordinator(
            promptAsync: _ =>
            {
                promptCalls++;
                return Task.FromResult(UpdatePromptDecision.Cancel);
            },
            applyAsync: _ =>
            {
                applyCalls++;
                return Task.FromResult(true);
            },
            notifyDeferred: _ => { });

        var started = await coordinator.TryApplyAvailableUpdateAsync(CreateApplicableUpdate(), showPrompt: true, notifyWhenDeferred: true);

        Assert.False(started);
        Assert.Equal(1, promptCalls);
        Assert.Equal(0, applyCalls);
    }

    [Fact]
    public async Task ConfirmPath_InvokesApplyPath()
    {
        var applyCalls = 0;

        var coordinator = new UpdateApplyCoordinator(
            promptAsync: _ => Task.FromResult(UpdatePromptDecision.Update),
            applyAsync: _ =>
            {
                applyCalls++;
                return Task.FromResult(true);
            },
            notifyDeferred: _ => { });

        var started = await coordinator.TryApplyAvailableUpdateAsync(CreateApplicableUpdate(), showPrompt: true, notifyWhenDeferred: true);

        Assert.True(started);
        Assert.Equal(1, applyCalls);
    }

    [Fact]
    public async Task CancelPath_DoesNotApplyAndDefersNotification()
    {
        var applyCalls = 0;
        var deferredNotifications = 0;

        var coordinator = new UpdateApplyCoordinator(
            promptAsync: _ => Task.FromResult(UpdatePromptDecision.Cancel),
            applyAsync: _ =>
            {
                applyCalls++;
                return Task.FromResult(true);
            },
            notifyDeferred: _ => deferredNotifications++);

        var started = await coordinator.TryApplyAvailableUpdateAsync(CreateApplicableUpdate(), showPrompt: true, notifyWhenDeferred: true);

        Assert.False(started);
        Assert.Equal(0, applyCalls);
        Assert.Equal(1, deferredNotifications);
    }

    [Fact]
    public async Task UnavailablePromptHost_DoesNotApplyAndDefersSafely()
    {
        var applyCalls = 0;
        var deferredNotifications = 0;

        var coordinator = new UpdateApplyCoordinator(
            promptAsync: _ => Task.FromResult(UpdatePromptDecision.Unavailable),
            applyAsync: _ =>
            {
                applyCalls++;
                return Task.FromResult(true);
            },
            notifyDeferred: _ => deferredNotifications++);

        var started = await coordinator.TryApplyAvailableUpdateAsync(CreateApplicableUpdate(), showPrompt: true, notifyWhenDeferred: true);

        Assert.False(started);
        Assert.Equal(0, applyCalls);
        Assert.Equal(1, deferredNotifications);
    }

    [Fact]
    public async Task ManualAboutFlow_TriggersSharedApplyPathAfterDetection()
    {
        var updateService = new Mock<IUpdateService>(MockBehavior.Strict);
        var checkForUpdateUseCase = new Mock<CheckForUpdateUseCase>(updateService.Object, "1.0.0")
        {
            CallBase = false
        };

        checkForUpdateUseCase
            .Setup(useCase => useCase.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UpdateCheckResult>.Ok(CreateApplicableUpdate()));

        var applyCalls = 0;
        var sharedCoordinator = new UpdateApplyCoordinator(
            promptAsync: _ => Task.FromResult(UpdatePromptDecision.Update),
            applyAsync: _ =>
            {
                applyCalls++;
                return Task.FromResult(true);
            },
            notifyDeferred: _ => { });

        var startupStarted = await sharedCoordinator.TryApplyAvailableUpdateAsync(CreateApplicableUpdate(), showPrompt: true, notifyWhenDeferred: true);
        Assert.True(startupStarted);

        var viewModel = new AboutViewModel(
            checkForUpdateUseCase.Object,
            result => sharedCoordinator.TryApplyAvailableUpdateAsync(result, showPrompt: true, notifyWhenDeferred: false));

        await viewModel.CheckForUpdateCommand.ExecuteAsync(null);
        Assert.True(viewModel.CanInstallUpdate);

        await viewModel.InstallUpdateCommand.ExecuteAsync(null);

        Assert.Equal(2, applyCalls);
    }

    [Fact]
    public async Task ManualAboutFlow_NoApplicableUpdate_DoesNotExposeOrTriggerInstall()
    {
        var updateService = new Mock<IUpdateService>(MockBehavior.Strict);
        var checkForUpdateUseCase = new Mock<CheckForUpdateUseCase>(updateService.Object, "1.0.0")
        {
            CallBase = false
        };

        checkForUpdateUseCase
            .Setup(useCase => useCase.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UpdateCheckResult>.Ok(UpdateCheckResult.NoUpdate()));

        var applyCalls = 0;
        var viewModel = new AboutViewModel(
            checkForUpdateUseCase.Object,
            _ =>
            {
                applyCalls++;
                return Task.FromResult(true);
            });

        await viewModel.CheckForUpdateCommand.ExecuteAsync(null);

        Assert.False(viewModel.CanInstallUpdate);
        Assert.Equal(Visibility.Collapsed, viewModel.InstallButtonVisibility);
        Assert.Equal("Ya tenés la última versión.", viewModel.UpdateStatusMessage);
        Assert.False(viewModel.InstallUpdateCommand.CanExecute(null));

        await viewModel.InstallUpdateCommand.ExecuteAsync(null);

        Assert.Equal(0, applyCalls);
        Assert.Equal("No hay una actualización lista para instalar.", viewModel.UpdateStatusMessage);
    }

    [Fact]
    public async Task ManualAboutFlow_NonApplicablePayload_DoesNotExposeOrTriggerInstall()
    {
        var updateService = new Mock<IUpdateService>(MockBehavior.Strict);
        var checkForUpdateUseCase = new Mock<CheckForUpdateUseCase>(updateService.Object, "1.0.0")
        {
            CallBase = false
        };

        var nonApplicableUpdate = UpdateCheckResult.Available(
            version: "1.2.3",
            url: string.Empty,
            sha256: string.Empty);

        checkForUpdateUseCase
            .Setup(useCase => useCase.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UpdateCheckResult>.Ok(nonApplicableUpdate));

        var applyCalls = 0;
        var viewModel = new AboutViewModel(
            checkForUpdateUseCase.Object,
            _ =>
            {
                applyCalls++;
                return Task.FromResult(true);
            });

        await viewModel.CheckForUpdateCommand.ExecuteAsync(null);

        Assert.False(viewModel.CanInstallUpdate);
        Assert.Equal(Visibility.Collapsed, viewModel.InstallButtonVisibility);
        Assert.Equal("Actualización disponible: v1.2.3", viewModel.UpdateStatusMessage);
        Assert.False(viewModel.InstallUpdateCommand.CanExecute(null));

        await viewModel.InstallUpdateCommand.ExecuteAsync(null);

        Assert.Equal(0, applyCalls);
        Assert.Equal("No hay una actualización lista para instalar.", viewModel.UpdateStatusMessage);
    }

    private static UpdateCheckResult CreateApplicableUpdate()
        => UpdateCheckResult.Available(
            version: "1.2.3",
            url: "https://example.com/mgg-pulse-1.2.3.exe",
            sha256: "ABCDEF1234567890");
}
