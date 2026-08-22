using Microsoft.JSInterop;

namespace Portfolio.Services;

/// <summary>
/// The whole JavaScript surface of the app, behind one typed facade over
/// <c>wwwroot/js/site.js</c>.
///
/// Every call is guarded. A blocked module load, a browser without
/// IntersectionObserver, or a disposed circuit must degrade to a fully readable
/// page rather than throw, the content is complete without any of this, only
/// the motion is missing.
/// </summary>
public sealed class JsInterop : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly Task<IJSObjectReference> _moduleTask;

    public JsInterop(IJSRuntime js)
    {
        _js = js;
        _moduleTask = _js.InvokeAsync<IJSObjectReference>("import", "./js/site.js").AsTask();
    }

    private async ValueTask<IJSObjectReference?> ModuleAsync()
    {
        try { return await _moduleTask; }
        catch { return null; }
    }

    private async ValueTask CallAsync(string fn, params object?[] args)
    {
        var module = await ModuleAsync();
        if (module is null) return;
        try { await module.InvokeVoidAsync(fn, args); }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
        catch (JSException) { }
    }

    private async ValueTask<T?> CallAsync<T>(string fn, params object?[] args)
    {
        var module = await ModuleAsync();
        if (module is null) return default;
        try { return await module.InvokeAsync<T>(fn, args); }
        catch (JSDisconnectedException) { return default; }
        catch (ObjectDisposedException) { return default; }
        catch (JSException) { return default; }
    }

    /* --- theme ---------------------------------------------------------- */

    /// <summary>The visitor's stored choice, or <c>null</c> if they've never made one.</summary>
    public ValueTask<string?> ReadStoredThemeAsync() => CallAsync<string>("readStoredTheme");

    public async ValueTask<bool> PrefersDarkAsync() => await CallAsync<bool>("prefersDark");

    public ValueTask ApplyThemeAsync(string theme) => CallAsync("applyTheme", theme);

    /* --- presentation --------------------------------------------------- */

    /// <summary>Re-points the reveal observer at anything newly rendered. Cheap; idempotent.</summary>
    public ValueTask ScanRevealsAsync() => CallAsync("scanReveals");

    public ValueTask WatchHeaderAsync() => CallAsync("watchHeader");

    public ValueTask MarkReadyAsync() => CallAsync("markReady");

    public ValueTask ScrollToTopAsync() => CallAsync("scrollToTop");

    public ValueTask<bool> ScrollToIdAsync(string id) => CallAsync<bool>("scrollToId", id);

    /* --- case-study scrollspy ------------------------------------------- */

    /// <summary>Returns a token; pass it to <see cref="StopScrollSpyAsync"/> so a late
    /// teardown from an outgoing page cannot cancel the incoming page's spy.</summary>
    public ValueTask<int> StartScrollSpyAsync<T>(string[] ids, DotNetObjectReference<T> reference)
        where T : class => CallAsync<int>("startScrollSpy", ids, reference);

    public ValueTask StopScrollSpyAsync(int token) => CallAsync("stopScrollSpy", token);

    /* --- runtime telemetry ---------------------------------------------- */

    public ValueTask<BootStats?> GetBootStatsAsync() => CallAsync<BootStats>("bootStats");

    public async ValueTask DisposeAsync()
    {
        var module = await ModuleAsync();
        if (module is null) return;
        try { await module.DisposeAsync(); }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
        catch (JSException) { }
    }
}

/// <summary>
/// Real numbers from the navigation timing API. Shown in the hero because a
/// portfolio that claims to be fast should be willing to print its own figures.
/// </summary>
public sealed record BootStats(int BootMs, int TransferKb, int Cores, string Wasm);
