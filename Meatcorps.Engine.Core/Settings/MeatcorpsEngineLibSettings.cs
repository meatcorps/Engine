namespace Meatcorps.Engine.Core.Settings;

public static class MeatcorpsEngineLibSettings
{
    public static bool IsDebug { get; set; }

    public static void Init()
    {
        // This is only works internally 
#if DEBUG
        IsDebug = true;
#endif
    }
}