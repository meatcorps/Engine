using Meatcorps.Engine.Session;
using Meatcorps.Engine.Session.Data;
using Meatcorps.Engine.Session.Extensions;
using Meatcorps.Engine.Session.Factories;
using Meatcorps.Engine.Session.Modules;

namespace Meatcorps.Game.KillTheSkulls.GameEnums;

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
                    .RegisterItemByValue(GamePlayerData.Lives, 5)
                    .RegisterItemByValue(GamePlayerData.Died, 0)
                    .RegisterItemByValue(GamePlayerData.TotalHits, 0)
                    .RegisterItemByValue(GamePlayerData.TotalMissed, 0)
                    .RegisterItemByValue(GamePlayerData.Streak, 0)
                    .RegisterItemByValue(GamePlayerData.MaxStreak, 0)
                )
                .RegisterTracker(new SessionDebugger<GameSessionData, GamePlayerData>())
        );
    }
}