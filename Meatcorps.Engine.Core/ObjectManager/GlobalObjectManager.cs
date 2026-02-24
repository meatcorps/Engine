namespace Meatcorps.Engine.Core.ObjectManager;

/// <summary>
/// Provides the process-wide singleton ObjectManager instance. All engine modules and game code share this registry unless a scoped ObjectManager is explicitly passed.
/// </summary>
public static class GlobalObjectManager
{
    /// <summary>The process-wide singleton ObjectManager.</summary>
    public static ObjectManager ObjectManager { get; } = new ObjectManager();
}