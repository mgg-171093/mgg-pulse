using MGG.Pulse.Domain.Entities;
using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.Ports;
using MGG.Pulse.Domain.ValueObjects;
using MGG.Pulse.UI.ViewModels;
using Moq;
using Xunit;

namespace MGG.Pulse.Tests.UI.ViewModels;

public class AppearanceViewModelTests
{
    [Fact]
    public async Task ApplyThemeSelectionAsync_WhenAutoSelected_PersistsAutoAndFlagsSelection()
    {
        var config = new SimulationConfig(
            mode: SimulationMode.Intelligent,
            interval: new IntervalRange(30, 60),
            inputType: InputType.Mouse);

        var configRepository = new Mock<IConfigRepository>();
        configRepository
            .Setup(repository => repository.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        var themeService = new Mock<IThemeService>();
        themeService.SetupGet(service => service.CurrentTheme).Returns("Dark");

        var viewModel = new AppearanceViewModel(themeService.Object, configRepository.Object);

        await viewModel.ApplyThemeSelectionAsync("Auto");

        Assert.True(viewModel.IsAutoTheme);
        Assert.True(viewModel.ShowRestartBanner);
        themeService.Verify(service => service.ApplyTheme(It.IsAny<string>()), Times.Never);
        configRepository.Verify(repository => repository.SaveAsync(
            It.Is<SimulationConfig>(saved => saved.AppearanceTheme == "Auto"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void RestartCommand_WhenExecuted_InvokesRestartAction()
    {
        var configRepository = new Mock<IConfigRepository>();
        var themeService = new Mock<IThemeService>();
        themeService.SetupGet(service => service.CurrentTheme).Returns("Dark");

        var restartCalls = 0;
        var viewModel = new AppearanceViewModel(themeService.Object, configRepository.Object, () => restartCalls++);

        viewModel.RestartCommand.Execute(null);

        Assert.Equal(1, restartCalls);
    }

    [Fact]
    public void Constructor_UsesCurrentThemeFromThemeService()
    {
        var configRepository = new Mock<IConfigRepository>();
        var themeService = new Mock<IThemeService>();
        themeService.SetupGet(service => service.CurrentTheme).Returns("Light");

        var viewModel = new AppearanceViewModel(themeService.Object, configRepository.Object);

        Assert.Equal("Light", viewModel.SelectedTheme);
        Assert.True(viewModel.IsLightTheme);
        Assert.False(viewModel.IsDarkTheme);
        Assert.False(viewModel.IsAutoTheme);
    }

    [Theory]
    [InlineData("Light", "Light")]
    [InlineData("Sepia", "Dark")]
    public async Task ApplyThemeSelectionAsync_PersistsThemeThroughConfigFlow(string selectedTheme, string expectedPersistedTheme)
    {
        var config = new SimulationConfig(
            mode: SimulationMode.Intelligent,
            interval: new IntervalRange(30, 60),
            inputType: InputType.Mouse);

        var configRepository = new Mock<IConfigRepository>();
        configRepository
            .Setup(repository => repository.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        var themeService = new Mock<IThemeService>();
        themeService.SetupGet(service => service.CurrentTheme).Returns("Dark");

        var viewModel = new AppearanceViewModel(themeService.Object, configRepository.Object);

        await viewModel.ApplyThemeSelectionAsync(selectedTheme);

        themeService.Verify(service => service.ApplyTheme(It.IsAny<string>()), Times.Never);
        Assert.True(viewModel.ShowRestartBanner);
        configRepository.Verify(repository => repository.SaveAsync(
            It.Is<SimulationConfig>(saved => saved.AppearanceTheme == expectedPersistedTheme),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
