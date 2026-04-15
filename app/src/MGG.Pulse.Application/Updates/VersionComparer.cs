namespace MGG.Pulse.Application.Updates;

/// <summary>
/// Pure function helper for semantic version comparison.
/// Extracted from <see cref="CheckForUpdateUseCase"/> for isolated testability.
/// </summary>
public static class VersionComparer
{
    /// <summary>
    /// Returns <c>true</c> when <paramref name="candidate"/> is strictly
    /// greater than <paramref name="baseline"/> using semantic-version ordering.
    /// Falls back to string equality when parsing fails.
    /// </summary>
    public static bool IsNewer(string candidate, string baseline)
    {
        if (Version.TryParse(candidate, out var candidateVer) &&
            Version.TryParse(baseline,  out var baselineVer))
        {
            return candidateVer > baselineVer;
        }

        // fallback: non-parseable strings are never considered newer
        return false;
    }
}
