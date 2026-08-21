namespace Portfolio.Services;

/// <summary>
/// Owns the light/dark choice. The initial value is already applied by an inline
/// script in <c>index.html</c> before first paint; this service reads it back so
/// C# and the DOM agree, then owns every change after that.
/// </summary>
public sealed class ThemeService
{
    public const string Light = "light";
    public const string Dark = "dark";

    private readonly JsInterop _interop;
    private bool _initialised;

    public ThemeService(JsInterop interop) => _interop = interop;

    public string Current { get; private set; } = Light;

    public bool IsDark => Current == Dark;

    /// <summary>Raised after <see cref="Current"/> changes, so components can re-render.</summary>
    public event Action? Changed;

    public async Task InitialiseAsync()
    {
        if (_initialised) return;
        _initialised = true;

        var stored = await _interop.ReadStoredThemeAsync();
        Current = stored is Light or Dark
            ? stored
            : await _interop.PrefersDarkAsync() ? Dark : Light;

        Changed?.Invoke();
    }

    public async Task ToggleAsync() => await SetAsync(IsDark ? Light : Dark);

    public async Task SetAsync(string theme)
    {
        if (theme != Light && theme != Dark) return;
        if (theme == Current && _initialised) return;

        Current = theme;
        await _interop.ApplyThemeAsync(theme);
        Changed?.Invoke();
    }
}
