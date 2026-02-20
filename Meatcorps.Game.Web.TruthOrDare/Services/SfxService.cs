using Microsoft.JSInterop;

namespace Meatcorps.Game.Web.TruthOrDare.Services;

public sealed class SfxService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private IJSObjectReference? _mod;
    public SfxService(IJSRuntime js) => _js = js;

    private async Task<IJSObjectReference> JSReference() =>
        _mod ??= await _js.InvokeAsync<IJSObjectReference>("import", "./js/sfx.js");

    public async Task InitAsync() => _ = (await JSReference()).InvokeVoidAsync("init");

    public async Task PreloadAsync(params string[] urls)
        => _ = (await JSReference()).InvokeVoidAsync("preload", (object)urls);

    public async Task PlayAsync(string url, float volume = 1f)
        => _ = (await JSReference()).InvokeVoidAsync("play", url, volume);

    public async ValueTask DisposeAsync()
    {
        if (_mod is not null) await _mod.DisposeAsync();
    }
}