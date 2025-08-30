namespace Meatcorps.Game.Pacman.GameEnums;

public enum CollisionLayer: uint
{
    PacMan = 1,
    Wall = 2,
    Ghost = 4,
    Items = 8,
    OneWay = 16,
}