namespace Meatcorps.Engine.Arcade.Interfaces;

public interface IPlayerCheckin : IArcadeGameInfo
{
    bool IsPlayerCheckedIn(int player, out string name);
    string GetPlayerName(int player);
    void SignPlayerOut(int player);
    void SetTotalPlayerSessions(int total);
    int TotalPlayers { get; }
}