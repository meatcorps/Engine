using System.Numerics;
using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Game.Pacman.Data;

public readonly record struct MapItem
{
    public PointInt Position { get; init; }
    public required PointInt[] Directions { get; init; }
    public bool Walkable { get; init; }
    public bool OneWay { get; init; }
    public bool GhostHome { get; init; }
}