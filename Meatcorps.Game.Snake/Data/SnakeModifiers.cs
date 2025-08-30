namespace Meatcorps.Game.Snake.Data;

public class SnakeModifiers
{
    public float ScoreModifier { get; set; } = 1;
    public float SpeedModifier { get; set; } = 1;
    public bool PassThroughWalls { get; set; }
    public bool RotProof { get; set; }

    public void Reset()
    {
        ScoreModifier = 1;
        PassThroughWalls = false;
        RotProof = false;
    }
}