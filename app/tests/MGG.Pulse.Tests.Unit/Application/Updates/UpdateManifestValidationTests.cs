using MGG.Pulse.Application.Updates;
using MGG.Pulse.Domain.Updates;
using Xunit;

namespace MGG.Pulse.Tests.Unit.Application.Updates;

/// <summary>
/// RED phase: Tests for UpdateManifest validation logic.
/// These must fail until Task 2.5 (GREEN) validates manifest fields.
/// </summary>
public class UpdateManifestValidationTests
{
    // ──────────────────────────────────────────────
    //  Missing required fields
    // ──────────────────────────────────────────────

    [Fact]
    public void Validate_WhenVersionIsEmpty_ReturnsInvalid()
    {
        var manifest = new UpdateManifest { Version = "", Url = "https://example.com/setup.exe", Sha256 = "abc" };

        var result = ManifestValidator.Validate(manifest);

        Assert.False(result.IsValid);
        Assert.Contains("Version", result.Errors);
    }

    [Fact]
    public void Validate_WhenUrlIsEmpty_ReturnsInvalid()
    {
        var manifest = new UpdateManifest { Version = "1.0.0", Url = "", Sha256 = "abc" };

        var result = ManifestValidator.Validate(manifest);

        Assert.False(result.IsValid);
        Assert.Contains("Url", result.Errors);
    }

    [Fact]
    public void Validate_WhenSha256IsEmpty_ReturnsInvalid()
    {
        var manifest = new UpdateManifest { Version = "1.0.0", Url = "https://example.com/setup.exe", Sha256 = "" };

        var result = ManifestValidator.Validate(manifest);

        Assert.False(result.IsValid);
        Assert.Contains("Sha256", result.Errors);
    }

    // ──────────────────────────────────────────────
    //  Invalid semver
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("not-a-version")]
    [InlineData("1.x.0")]
    [InlineData("")]
    public void Validate_WhenVersionIsNotParseable_ReturnsInvalidWithVersionError(string version)
    {
        var manifest = new UpdateManifest { Version = version, Url = "https://example.com/setup.exe", Sha256 = "abc" };

        var result = ManifestValidator.Validate(manifest);

        Assert.False(result.IsValid);
        Assert.Contains("Version", result.Errors);
    }

    // ──────────────────────────────────────────────
    //  Valid manifest — all fields present and parseable
    // ──────────────────────────────────────────────

    [Fact]
    public void Validate_WhenAllFieldsValid_ReturnsValid()
    {
        var manifest = new UpdateManifest
        {
            Version = "1.2.3",
            Url     = "https://example.com/MGGPulse-Setup-1.2.3.exe",
            Sha256  = "abc123def456"
        };

        var result = ManifestValidator.Validate(manifest);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    // ──────────────────────────────────────────────
    //  Multiple invalid fields — all errors reported
    // ──────────────────────────────────────────────

    [Fact]
    public void Validate_WhenMultipleFieldsInvalid_ReturnsAllErrors()
    {
        var manifest = new UpdateManifest { Version = "", Url = "", Sha256 = "" };

        var result = ManifestValidator.Validate(manifest);

        Assert.False(result.IsValid);
        Assert.Contains("Version", result.Errors);
        Assert.Contains("Url", result.Errors);
        Assert.Contains("Sha256", result.Errors);
        Assert.Equal(3, result.Errors.Count);
    }
}
