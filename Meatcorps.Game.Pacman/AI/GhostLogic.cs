using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Pathfinding.ResourceBinder;
using Meatcorps.Game.Pacman.Data;
using Meatcorps.Game.Pacman.GameObjects;
using Meatcorps.Game.Pacman.GameObjects.GhostManagers;
using Raylib_cs;

namespace Meatcorps.Game.Pacman.AI;

public abstract class GhostLogic: BaseGridDistanceResource
{
    protected readonly LevelData LevelData;
    public override Rect Bounds { get; }
    public abstract int TimeoutBeforeStart { get; }
    public PointInt Corner { get; set; }
    private List<PointInt> _homes { get; } = new();

    public GhostLogic(LevelData levelData)
    {
        LevelData = levelData;
        Bounds = new Rect(PointInt.Zero, new PointInt(levelData.LevelWidth, levelData.LevelHeight));

        var counter = 0;
        foreach (var itemLevelData in LevelData.Map.Entities.Values)
        {
            if (itemLevelData.GhostHome) {
                _homes.Add(itemLevelData.Position);;
            }
        }
        _homes.Sort((x, b) => x.Y > b.Y ? -1 : 1);
    }

    public PointInt GetTarget(GhostState state)
    {
        if (state == GhostState.Eaten)
        {
            if (LevelData.HomeTaken.TryGetValue(this, out var homeAddressFound))
                return homeAddressFound;
            
            foreach (var homeAddress in _homes)
            {
                if (!LevelData.HomeTaken.ContainsValue(homeAddress)) {
                    return homeAddress;
                }
            }
        }

        if (LevelData.GhostScared && LevelData.TargetPacman != null)
            return LevelData.WorldToCell(LevelData.TargetPacman.Body.Position + new Vector2(LevelData.GridSize / 2f, LevelData.GridSize / 2f));
        
        if (state == GhostState.Scatter)
            return Corner;
        
        return OnGetChaseTarget();
    }

    protected virtual PointInt OnGetChaseTarget()
    {
        return LevelData.WorldToCell(LevelData.TargetPacman!.Body.Position + new Vector2(LevelData.GridSize / 2f, LevelData.GridSize / 2f));
    }

    public int GetStartDistance()
    {
        if (LevelData.GhostScared)
            return 0;
        
        return OnGetStartDistance();
    }
    
    protected virtual int OnGetStartDistance()
    {
        return 0;
    }

    protected override PointInt MutatePosition(PointInt from)
    {
        return from.Warp(LevelData.LevelWidth, LevelData.LevelHeight);
    }

    protected override bool OnIsValid(PointInt point)
    {
        if (!LevelData.Map.TryGet(point, out var item))
            return false;
        
        return item.Walkable;
    }
    
    protected override bool OnTryGet(PointInt point, PointInt direction, out int additionalCost)
    {
        additionalCost = 0;
        if (!LevelData.Map.TryGet(point, out var item))
            return false;
        
        if (point.X == 0 || point.X == LevelData.LevelWidth - 1 || point.Y == 0 || point.Y == LevelData.LevelHeight - 1)
            additionalCost = 5;
        
        return item.Walkable;
    }
}