namespace MGG.Pulse.Domain.Updates;

/// <summary>
/// Represents the content of a <c>latest.json</c> release manifest.
/// Used by <see cref="MGG.Pulse.Domain.Ports.IUpdateService"/> to communicate
/// the available update to the Application layer.
/// </summary>
public sealed class UpdateManifest
{
    /// <summary>Semantic version string of the available release (e.g. "1.2.0").</summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>Direct URL to the versioned installer executable.</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>SHA-256 hex digest of the installer file for integrity verification.</summary>
    public string Sha256 { get; init; } = string.Empty;

    /// <summary>Optional human-readable release notes or changelog excerpt.</summary>
    public string? Notes { get; init; }
}
