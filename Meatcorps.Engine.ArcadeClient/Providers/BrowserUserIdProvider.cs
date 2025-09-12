using Meatcorps.Engine.ArcadeClient.Interfaces;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.ObjectManager;

namespace Meatcorps.Engine.ArcadeClient.Providers;

using Microsoft.JSInterop;

public sealed class BrowserUserIdProvider(IJSRuntime js) : IAsyncDisposable, IUserIdProvider
{
    private readonly IJSRuntime _js = js;
    private IJSObjectReference? _module;
    private string? _cached;

    public string? CurrentId => _cached;
    public bool IsAdmin { get; private set; }

    public async ValueTask<string> GetUserIdAsync()
    {
        if (_cached is not null) return _cached;

        // Import the module on first use (must be called after hydration)
        _module ??= await _js.InvokeAsync<IJSObjectReference>(
            "import", "/js/localStorageHelper.js");

        _cached = await _module.InvokeAsync<string>(
            "getOrCreateGuid", "myAppUserId");
  
        return _cached;
    }
    
    public async ValueTask<bool> IsAdminAsync()
    {
        // Import the module on first use (must be called after hydration)
        _module ??= await _js.InvokeAsync<IJSObjectReference>(
            "import", "/js/localStorageHelper.js");
        
        var key = await _module.InvokeAsync<string>(
            "getKey", "adminKey");
        
        if (key == GlobalObjectManager.ObjectManager.Get<IUniversalConfig>()!.GetOrDefault("Users", "AdminKey", "admin"))
            IsAdmin = true;
        
        return IsAdmin;
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
            await _module.DisposeAsync();
    }
}