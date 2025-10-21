using Microsoft.JSInterop;

namespace Meatcorps.Game.Web.TruthOrDare.Services;

public sealed class SfxService : IAsyncDisposable
{
    private readonly IJSRuntime js;
    private IJSObjectReference? mod;
    public SfxService(IJSRuntime js) => this.js = js;

    private async Task<IJSObjectReference> JSReference() =>
        mod ??= await js.InvokeAsync<IJSObjectReference>("import", "./js/sfx.js");

    public async Task InitAsync() => (await JSReference()).InvokeVoidAsync("init");

    public async Task PreloadAsync(params string[] urls)
        => (await JSReference()).InvokeVoidAsync("preload", (object)urls);

    public async Task PlayAsync(string url, float volume = 1f)
        => (await JSReference()).InvokeVoidAsync("play", url, volume);

    public async ValueTask DisposeAsync()
    {
        if (mod is not null) await mod.DisposeAsync();
    }
}