using Meatcorps.Engine.Session;
using Meatcorps.Engine.Session.Data;
using Meatcorps.Engine.Session.Extensions;
using Meatcorps.Engine.Session.Factories;
using Meatcorps.Engine.Session.Modules;

namespace Meatcorps.Engine.Arcade.Leaderboard.GameEnums;

public static class GameSession
{
    public static void Load()
    {
        SessionModule.Create(
            new SessionFactory<GameSessionData, GamePlayerData>()
                .SetMaxPlayers(2)
                .SetSessionDataFactory(() => new SessionDataBag<GameSessionData>()
                    .RegisterItemByValue(GameSessionData.CurrentLevel, 1)
                )
                .SetPlayerSessionDataFactory(() => new SessionDataBag<GamePlayerData>()
                    .RegisterItemByValue(GamePlayerData.Score, 0)
                    .RegisterItemByValue(GamePlayerData.Lives, 1)
                    .RegisterItemByValue(GamePlayerData.Died, 0)
                )
                .RegisterTracker(new SessionDebugger<GameSessionData, GamePlayerData>())
        );
    }
}