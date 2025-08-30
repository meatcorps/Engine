using Meatcorps.Engine.Session;
using Meatcorps.Engine.Session.Data;
using Meatcorps.Engine.Session.Extensions;
using Meatcorps.Engine.Session.Factories;
using Meatcorps.Engine.Session.Modules;

namespace Meatcorps.Game.Snake.Resources;

public static class SnakeSession
{
    public static void Load()
    {
        SessionModule.Create(
            new SessionFactory<SnakeSessionData, SnakePlayerData>()
                .SetMaxPlayers(2)
                .SetSessionDataFactory(() => new SessionDataBag<SnakeSessionData>()
                    .RegisterItemByValue(SnakeSessionData.CurrentLevel, 1)
                )
                .SetPlayerSessionDataFactory(() => new SessionDataBag<SnakePlayerData>()
                    .RegisterItemByValue(SnakePlayerData.Score, 0)
                    .RegisterItemByValue(SnakePlayerData.PickupsTaken, 0)
                    .RegisterItemByValue(SnakePlayerData.SnakeLength, 0)
                    .RegisterItemByValue(SnakePlayerData.Lives, 1)
                    .RegisterItemByValue(SnakePlayerData.Died, 0)
                    .RegisterItemByValue(SnakePlayerData.MeatEaten, 0)
                )
                .RegisterTracker(new SessionDebugger<SnakeSessionData, SnakePlayerData>())
        );
    } 
}

public enum SnakeSessionData
{
    CurrentLevel
}

public enum SnakePlayerData
{
    Score,
    PickupsTaken,
    SnakeLength,
    Lives,
    Died,
    MeatEaten,
}