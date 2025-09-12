namespace Meatcorps.Engine.ArcadeClient.Interfaces;

public interface IUserIdProvider
{
    /// Returns the same ID across reloads in the same browser/profile.
    ValueTask<string> GetUserIdAsync();

    /// Optional: returns the last-read value without JS (null until first call).
    string? CurrentId { get; }
    
    bool IsAdmin { get; }

    ValueTask<bool> IsAdminAsync();
}
