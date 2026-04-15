using MGG.Pulse.Domain.Ports;
using MGG.Pulse.Domain.Updates;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MGG.Pulse.Infrastructure.Update;

/// <summary>
/// Fetches the latest update manifest from a remote <c>latest.json</c> file hosted
/// on GitHub Releases (or any compatible static JSON endpoint).
///
/// Expected JSON schema:
/// <code>
/// { "version": "1.2.0", "url": "https://...", "sha256": "abc123...", "notes": "..." }
/// </code>
/// </summary>
public sealed class GithubReleaseUpdateService : IUpdateService
{
    private readonly HttpClient _httpClient;
    private readonly string _manifestUrl;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <param name="httpClient">Injected HttpClient (configured via IHttpClientFactory or direct).</param>
    /// <param name="manifestUrl">URL of the <c>latest.json</c> manifest.</param>
    public GithubReleaseUpdateService(HttpClient httpClient, string manifestUrl)
    {
        _httpClient  = httpClient  ?? throw new ArgumentNullException(nameof(httpClient));
        _manifestUrl = manifestUrl ?? throw new ArgumentNullException(nameof(manifestUrl));
    }

    /// <inheritdoc/>
    public async Task<UpdateManifest> FetchManifestAsync(CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(_manifestUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var dto = await JsonSerializer.DeserializeAsync<ManifestDto>(stream, _jsonOptions, cancellationToken)
                  ?? throw new InvalidOperationException("Manifest JSON was empty or null.");

        return new UpdateManifest
        {
            Version = dto.Version ?? string.Empty,
            Url     = dto.Url     ?? string.Empty,
            Sha256  = dto.Sha256  ?? string.Empty,
            Notes   = dto.Notes
        };
    }

    // Private DTO to avoid coupling UpdateManifest to JSON serialization concerns
    private sealed class ManifestDto
    {
        [JsonPropertyName("version")] public string? Version { get; init; }
        [JsonPropertyName("url")]     public string? Url     { get; init; }
        [JsonPropertyName("sha256")]  public string? Sha256  { get; init; }
        [JsonPropertyName("notes")]   public string? Notes   { get; init; }
    }
}
