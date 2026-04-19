using MGG.Pulse.Domain.Entities;
using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.ValueObjects;
using Xunit;

namespace MGG.Pulse.Tests.Core.Domain.Entities;

public class SimulationConfigTests
{
    [Fact]
    public void Default_WhenAccessed_UsesDarkAppearanceTheme()
    {
        var config = SimulationConfig.Default;

        Assert.Equal("Dark", config.AppearanceTheme);
    }

    [Fact]
    public void UpdateAppearanceTheme_WhenLightSelected_StoresNormalizedTheme()
    {
        var config = new SimulationConfig(
            mode: SimulationMode.Intelligent,
            interval: new IntervalRange(30, 60),
            inputType: InputType.Mouse);

        config.UpdateAppearanceTheme("light");

        Assert.Equal("Light", config.AppearanceTheme);
    }

    [Fact]
    public void UpdateAppearanceTheme_WhenAutoSelected_StoresAutoTheme()
    {
        var config = new SimulationConfig(
            mode: SimulationMode.Intelligent,
            interval: new IntervalRange(30, 60),
            inputType: InputType.Mouse);

        config.UpdateAppearanceTheme("Auto");

        Assert.Equal("Auto", config.AppearanceTheme);
    }

    [Fact]
    public void UpdateAppearanceTheme_WhenThemeIsInvalid_FallsBackToDark()
    {
        var config = new SimulationConfig(
            mode: SimulationMode.Intelligent,
            interval: new IntervalRange(30, 60),
            inputType: InputType.Mouse);

        config.UpdateAppearanceTheme("Sepia");

        Assert.Equal("Dark", config.AppearanceTheme);
    }
}
