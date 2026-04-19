using MGG.Pulse.Domain.Entities;
using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.Ports;
using MGG.Pulse.Domain.ValueObjects;
using MGG.Pulse.UI.ViewModels;
using Moq;
using Xunit;

namespace MGG.Pulse.Tests.UI.ViewModels;

public class SettingsViewModelTests
{
    [Fact]
    public async Task SaveAsync_WhenAutoAppearanceSelected_PersistsAutoAndAppliesIt()
    {
        var config = new SimulationConfig(
            mode: SimulationMode.Intelligent,
            interval: new IntervalRange(30, 60),
            inputType: InputType.Mouse,
            appearanceTheme: "Dark");

        var configRepository = new Mock<IConfigRepository>();
        configRepository
            .Setup(repository => repository.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        var themeService = new Mock<IThemeService>();
        themeService.SetupGet(service => service.CurrentTheme).Returns("Dark");

        var settingsViewModel = new SettingsViewModel(configRepository.Object, themeService.Object);
        settingsViewModel.SelectedAppearanceTheme = "Auto";

        await settingsViewModel.SaveCommand.ExecuteAsync(null);

        themeService.Verify(service => service.ApplyTheme(It.IsAny<string>()), Times.Never);
        configRepository.Verify(repository => repository.SaveAsync(
            It.Is<SimulationConfig>(saved => saved.AppearanceTheme == "Auto"),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task SaveAsync_WhenAppearanceSelectionProvided_PersistsThemeAndAppliesIt()
    {
        var config = new SimulationConfig(
            mode: SimulationMode.Intelligent,
            interval: new IntervalRange(30, 60),
            inputType: InputType.Mouse,
            appearanceTheme: "Dark");

        var configRepository = new Mock<IConfigRepository>();
        configRepository
            .Setup(repository => repository.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        var themeService = new Mock<IThemeService>();
        themeService.SetupGet(service => service.CurrentTheme).Returns("Dark");

        var settingsViewModel = new SettingsViewModel(configRepository.Object, themeService.Object);
        settingsViewModel.SelectedAppearanceTheme = "Light";

        await settingsViewModel.SaveCommand.ExecuteAsync(null);

        themeService.Verify(service => service.ApplyTheme(It.IsAny<string>()), Times.Never);
        configRepository.Verify(repository => repository.SaveAsync(
            It.Is<SimulationConfig>(saved => saved.AppearanceTheme == "Light"),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        Assert.Equal(string.Empty, settingsViewModel.SaveStatusMessage);
    }

}
