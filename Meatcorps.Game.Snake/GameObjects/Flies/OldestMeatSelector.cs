using System.Numerics;
using Meatcorps.Engine.Boids.Interfaces;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Game.Snake.Data;

namespace Meatcorps.Game.Snake.GameObjects.Flies;

public class OldestMeatSelector : ITargetSelector
{
    private readonly LevelData _level;
    private readonly bool _preferRottenFirst;

    public OldestMeatSelector(LevelData level, bool preferRottenFirst = false)
    {
        _level = level;
        _preferRottenFirst = preferRottenFirst;
    }

    public Vector2? GetTarget(float nowSeconds)
    {
        Consumable? best = null;
        float bestAge = float.MinValue;
        PointInt bestCell = default;

        Consumable? bestRotten = null;
        float bestRottenAge = float.MinValue;
        PointInt bestRottenCell = default;

        foreach (var kvp in _level.ConsumableGrid.Entities)
        {
            var cell = kvp.Key;
            var c = kvp.Value;

            if (!c.IsMeat)
            {
                continue;
            }

            var age = nowSeconds - c.SpawnTimeSeconds;

            if (_preferRottenFirst && c.IsRotten)
            {
                if (age > bestRottenAge)
                {
                    bestRotten = c;
                    bestRottenAge = age;
                    bestRottenCell = cell;
                }
                continue;
            }

            if (age > bestAge)
            {
                best = c;
                bestAge = age;
                bestCell = cell;
            }
        }

        if (_preferRottenFirst && bestRotten != null)
        {
            return _level.ToWorldPosition(bestRottenCell);
        }

        return best != null ? _level.ToWorldPosition(bestCell) + new Vector2((float)_level.GridSize / 2, (float)_level.GridSize / 2) : (Vector2?)null;
    }
}