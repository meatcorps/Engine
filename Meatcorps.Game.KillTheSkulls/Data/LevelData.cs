using Meatcorps.Game.KillTheSkulls.GameObjects;

namespace Meatcorps.Game.KillTheSkulls.Data;

public class LevelData
{
    public LevelRow[] LevelRows { get; private set; }
    
    public LevelData(LevelRow[] levelRows){
        LevelRows = levelRows;
    }

}

public class LevelRow
{
    public bool Miss { get; set; }
    public bool Hit { get; set; }
    public required LedBar LedBar { get; init; }
    public required Thunder Thunder { get; init; }
    public required Enemy Enemy { get; init; }
}