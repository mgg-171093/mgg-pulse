using MGG.Pulse.Domain.Updates;

namespace MGG.Pulse.Application.Updates;

/// <summary>Result of a manifest validation pass.</summary>
public sealed class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public IReadOnlyList<string> Errors { get; }

    private ValidationResult(List<string> errors) => Errors = errors;

    public static ValidationResult Ok()   => new([]);
    public static ValidationResult Fail(List<string> errors) => new(errors);
}

/// <summary>
/// Pure static validator for <see cref="UpdateManifest"/> fields.
/// Extracted so tests can exercise validation independently of the use case.
/// </summary>
public static class ManifestValidator
{
    /// <summary>
    /// Validates that required fields are present and Version is a parseable
    /// semantic version string (major.minor.patch).
    /// </summary>
    public static ValidationResult Validate(UpdateManifest manifest)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(manifest.Version) || !Version.TryParse(manifest.Version, out _))
        {
            errors.Add("Version");
        }

        if (string.IsNullOrWhiteSpace(manifest.Url))
        {
            errors.Add("Url");
        }

        if (string.IsNullOrWhiteSpace(manifest.Sha256))
        {
            errors.Add("Sha256");
        }

        return errors.Count == 0
            ? ValidationResult.Ok()
            : ValidationResult.Fail(errors);
    }
}
