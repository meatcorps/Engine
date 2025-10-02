using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Modules;
using Meatcorps.Engine.Arcade.RayLib.Services;
using Meatcorps.Engine.Core.Interfaces.Trackers;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;

namespace Meatcorps.Engine.Arcade.RayLib.Modules;

public static class ArcadeGameSystemModuleExtensions
{
    public static void SetIntroScene<T>(this ArcadeGameSystemModule _) where T : BaseScene
    {
        var tracker = new ArcadeGameStateTracker(GlobalObjectManager.ObjectManager.Get<ArcadeGame>()!);
        tracker.SetIntroScene<T>();
        GlobalObjectManager.ObjectManager.Add<ISceneSwitchTracker>(tracker);
    }
}