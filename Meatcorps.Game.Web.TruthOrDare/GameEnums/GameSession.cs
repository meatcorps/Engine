using Meatcorps.Engine.Session;
using Meatcorps.Engine.Session.Data;
using Meatcorps.Engine.Session.Extensions;
using Meatcorps.Engine.Session.Factories;
using Meatcorps.Engine.Session.Modules;

namespace Meatcorps.Game.Web.TruthOrDare.GameEnums;

public static class GameSession
{
    public static void Load()
    {
        SessionModule.Create(
            new SessionFactory<GameSessionData, GamePlayerData>()
                .SetMaxPlayers(1)
                .SetSessionDataFactory(() => new SessionDataBag<GameSessionData>()
                    .RegisterItemByValue(GameSessionData.CurrentLevel, 1)
                )
                .SetPlayerSessionDataFactory(() => new SessionDataBag<GamePlayerData>()
                    .RegisterItemByValue(GamePlayerData.Score, 0)
                    .RegisterItemByValue(GamePlayerData.Description, string.Empty)
                    .RegisterItemByValue(GamePlayerData.TruthOrDare, string.Empty)
                    .RegisterItemByValue(GamePlayerData.TotalHearts, 0)
                    .RegisterItemByValue(GamePlayerData.TotalSkulls, 0)
                    .RegisterItemByValue(GamePlayerData.TotalThumbDown, 0)
                    .RegisterItemByValue(GamePlayerData.TotalThumbUp, 0)
                )
                .RegisterTracker(new SessionDebugger<GameSessionData, GamePlayerData>())
        );
    }
}