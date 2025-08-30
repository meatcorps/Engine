using Meatcorps.Engine.Session.Data;

namespace Meatcorps.Engine.Session.Interfaces;

public interface ISessionTracker<TEnumSession, TEnumPlayer>
    where TEnumSession : Enum
    where TEnumPlayer : Enum 
{
    void SessionStarted(SessionSet<TEnumSession, TEnumPlayer> session);
    void SessionEnded(SessionSet<TEnumSession, TEnumPlayer> session);
    void PlayerJoined(SessionDataBag<TEnumSession> session, SessionDataBag<TEnumPlayer> player, int totalPlayers);
    void PlayerLeft(SessionDataBag<TEnumSession> session, SessionDataBag<TEnumPlayer> player);
}