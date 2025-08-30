using Meatcorps.Engine.Session.Data;
using Meatcorps.Engine.Session.Interfaces;
using Meatcorps.Engine.Session.ValueTypes;

namespace Meatcorps.Engine.Session;

public class SessionDebugger<TEnumSession, TEnumPlater>: ISessionTracker<TEnumSession, TEnumPlater> 
    where TEnumSession : Enum 
    where TEnumPlater : Enum
{
    public void SessionStarted(SessionSet<TEnumSession, TEnumPlater> session)
    {
        Console.WriteLine("Session Started");
    }

    public void SessionEnded(SessionSet<TEnumSession, TEnumPlater> session)
    {
        Console.WriteLine("Session Ended");
    }

    public void PlayerJoined(SessionDataBag<TEnumSession> session, SessionDataBag<TEnumPlater> player, int totalPlayers)
    {
        Console.WriteLine($"{player.Get<string>(PlayerDefaultTypes.PlayerName)} Joined!");
    }

    public void PlayerLeft(SessionDataBag<TEnumSession> session, SessionDataBag<TEnumPlater> player)
    {
        Console.WriteLine($"{player.Get<string>(PlayerDefaultTypes.PlayerName)} Leaved!");
    }
}