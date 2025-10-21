using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Enums;
using Meatcorps.Engine.Arcade.Interfaces;

namespace Meatcorps.Engine.QuestionTest;

public class SimplePlayerCheckin: IPlayerCheckin
{
    public ArcadeGame Game => new ArcadeGame
    {
        Code = 0,
        MaxPlayers = 1,
        Name = "TEST GAME",
        PricePoints = 1000,
        Description = "TEST GAME",
        State = GameState.Idle
    };
    
    public bool IsPlayerCheckedIn(int player, out string name)
    {
        name = "Test player";
        return true;
    }

    public string GetPlayerName(int player)
    {
        return "Test player";
    }

    public void SignPlayerOut(int player)
    {
        
    }

    public void SetTotalPlayerSessions(int total)
    {
    }

    public int TotalPlayers => 1;
}