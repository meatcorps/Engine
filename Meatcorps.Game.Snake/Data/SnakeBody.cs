using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Game.Snake.Data;

public struct SnakeBody
{
    public SnakeBodyType BodyType { get; set; }
    public PointInt Position { get; set; }
    public GameObjects.Snake GameObject { get; set; }

    public SnakeBody(GameObjects.Snake gameObject, PointInt position, SnakeBodyType bodyType)
    {
        BodyType = bodyType;
        Position = position;
        GameObject = gameObject;
    }
}