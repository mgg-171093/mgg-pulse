namespace MGG.Pulse.Domain.Ports;

public interface IThemeService
{
    public string CurrentTheme { get; }
    public void ApplyTheme(string theme);
    public string ResolveEffectiveTheme(string appearance);
}
