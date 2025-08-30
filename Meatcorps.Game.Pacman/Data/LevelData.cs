using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.GridSystem;
using Meatcorps.Game.Pacman.AI;
using Meatcorps.Game.Pacman.GameObjects;
using Raylib_cs;

namespace Meatcorps.Game.Pacman.Data;

public class LevelData
{
    public List<Player> Players { get; } = new();
    public Dictionary<GhostLogic, PointInt> HomeTaken { get; } = new();
    public int LevelWidth { get; }
    public int LevelHeight { get; }
    public int GridSize { get; }
    public float Speed { get; set; } = 75;
    public bool GhostScared { get; set; } = false;
    public bool GhostScaredResetTimer { get; set; } = false;
    public Player? TargetPacman { get; set; }
    public int CollectibleCount { get; set; } = 0;
    public int CollectiblesGone { get; set; } = 0;
    
    public int ChaseTime { get; set; } = 10000;
    public int ScatterTime { get; set; } = 10000;
    public int StayAtHomeTime { get; set; } = 3000;
    public int ScaredTime { get; set; } = 10000;
    public int TotalGhostEaten { get; set; } = 0;

    public bool AuthenticBug { get; set; } = true;
    public bool FreezePlayersAndGhosts { get; set; } = false;
    
    public SingleEntityGrid<MapItem> Map { get; } = new();
    public List<GhostBehaviour> Ghosts { get; } = new();
    public TargetSeekerGameObject? TargetSeeker { get; set; }

    public LevelData(int levelWidth = 39, int levelHeight = 19, int gridSize = 16)
    {
        LevelWidth = levelWidth;
        LevelHeight = levelHeight;
        GridSize = gridSize;
    }

    public Vector2 ToWorldPosition(in PointInt position, bool center = false)
    {
        if (center)
            return new Vector2(position.X * GridSize + (float)GridSize / 2,
                position.Y * GridSize + (float)GridSize / 2);
        return new Vector2(position.X * GridSize, position.Y * GridSize);
    }
    
    
    public Rect ToWorldRectangle(in PointInt position)
    {
        return new Rect(position.X * GridSize, position.Y * GridSize, GridSize, GridSize);
    }

    public PointInt WorldToCell(in Vector2 world)
    {
        var s = GridSize;
        var x = (int)MathF.Floor(world.X / s);
        var y = (int)MathF.Floor(world.Y / s);

        // clamp into level bounds
        if (x < 0) x = 0;
        if (y < 0) y = 0;
        if (x >= LevelWidth) x = LevelWidth - 1;
        if (y >= LevelHeight) y = LevelHeight - 1;

        return new PointInt(x, y);
    }
}