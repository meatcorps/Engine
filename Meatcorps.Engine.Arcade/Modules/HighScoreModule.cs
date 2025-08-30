using Meatcorps.Engine.Arcade.Services;
using Meatcorps.Engine.Core.ObjectManager;

namespace Meatcorps.Engine.Arcade.Modules;

public static class HighScoreModule
{
    public static void Load(int maxScores = 50)
    {
        GlobalObjectManager.ObjectManager.Register(new HighScoreService(maxScores));
    }
}