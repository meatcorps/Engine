using System.Numerics;
using Meatcorps.Game.GameStarter.GameObjects.Abstractions;
using Raylib_cs;

namespace Meatcorps.Game.GameStarter.GameObjects;

public class ExampleGameObject: ResourceGameObject
{
    protected override void OnInitialize()
    {
        // do something!
        Console.WriteLine("Hello world!");
        base.OnInitialize();
    }

    protected override void OnUpdate(float deltaTime)
    {
    }

    protected override void OnDraw()
    {
        Raylib.DrawTextEx(Fonts.GetFont(), "Hello world!", new Vector2(64, 64), 16, 1, Color.White);
        base.OnDraw();
    }

    protected override void OnDispose()
    {
    }
}