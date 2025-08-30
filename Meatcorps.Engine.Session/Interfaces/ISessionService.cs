namespace Meatcorps.Engine.Session;

public interface ISessionService
{
    void StartSession(int seed);
    void StartSession();
    bool TryPlayerJoin(string playerName);
    bool TryPlayerJoin(int playerId, string playerName);
    bool TryPlayerDrop(int playerId);
    void StopSession();
    void ResetSession();
    void ClearPlayers();
}