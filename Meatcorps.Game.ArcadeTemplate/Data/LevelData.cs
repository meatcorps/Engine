using System.Numerics;
using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Game.ArcadeTemplate.Data;

public class LevelData
{
    public int LevelWidth { get; }
    public int LevelHeight { get;  }
    public int GridSize { get; }
    
    public LevelData(int levelWidth = 38, int levelHeight = 20, int gridSize = 16)
    {
        LevelWidth = levelWidth;
        LevelHeight = levelHeight;
        GridSize = gridSize;
    }

    public Vector2 ToWorldPosition(in PointInt position, bool center = false)
    {
        if (center)
            return new Vector2(position.X * GridSize + (float)GridSize / 2, position.Y * GridSize + (float)GridSize / 2);
        return new Vector2(position.X * GridSize, position.Y * GridSize);
    }
    
    public PointInt WorldToCell(in Vector2 world)
    {
        var s = GridSize;
        var x = (int)MathF.Floor(world.X / s);
        var y = (int)MathF.Floor(world.Y / s);

        // clamp into level bounds
        if (x < 0) x = 0;
        if (y < 0) y = 0;
        if (x >= LevelWidth)  x = LevelWidth  - 1;
        if (y >= LevelHeight) y = LevelHeight - 1;

        return new PointInt(x, y);
    }
}