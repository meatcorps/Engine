namespace Meatcorps.Engine.Arcade.Interfaces;

public interface IArcadePointsMutator: IArcadeGameInfo
{
    public int GamePrice { get; }
    public int GetPoints(int player);
    public bool RequestPoints(int player, int points);
    
    public bool RequestPoints(int player);
    public void SubmitPoints(int player, int points);
}