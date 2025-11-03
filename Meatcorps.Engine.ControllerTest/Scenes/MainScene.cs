using Meatcorps.Engine.ControllerTest.GameObjects;
using Meatcorps.Engine.RayLib.Abstractions;

namespace Meatcorps.Engine.ControllerTest.Scenes;

public class MainScene: BaseScene
{
    protected override void OnInitialize()
    {
        AddGameObject(new MainGameObject());
    }

    protected override void OnUpdate(float deltaTime)
    {
    }

    protected override void OnDispose()
    {
    }
}