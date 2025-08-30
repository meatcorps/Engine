namespace Meatcorps.Engine.Core.ObjectManager;

public static class GlobalObjectManager
{
    public static ObjectManager ObjectManager { get; } = new ObjectManager();
}