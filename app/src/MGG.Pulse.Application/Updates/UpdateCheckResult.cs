namespace MGG.Pulse.Application.Updates;

/// <summary>
/// Result returned by <see cref="CheckForUpdateUseCase"/> describing
/// whether a newer release is available.
/// </summary>
public sealed class UpdateCheckResult
{
    /// <summary>True when a newer version was found in the manifest.</summary>
    public bool UpdateAvailable { get; init; }

    /// <summary>Version string from the manifest ("1.2.0") or null when no update exists.</summary>
    public string? AvailableVersion { get; init; }

    /// <summary>Download URL from the manifest, or null when no update exists.</summary>
    public string? DownloadUrl { get; init; }

    /// <summary>SHA-256 hex digest from the manifest for integrity check.</summary>
    public string? Sha256 { get; init; }

    /// <summary>Optional release notes from the manifest.</summary>
    public string? Notes { get; init; }

    public static UpdateCheckResult NoUpdate() => new() { UpdateAvailable = false };

    public static UpdateCheckResult Available(string version, string url, string sha256, string? notes = null)
        => new()
        {
            UpdateAvailable   = true,
            AvailableVersion  = version,
            DownloadUrl       = url,
            Sha256            = sha256,
            Notes             = notes
        };
}
