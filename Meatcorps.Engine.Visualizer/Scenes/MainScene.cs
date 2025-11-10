using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.Visualizer.GameObjects;

namespace Meatcorps.Engine.Visualizer.Scenes;

public class MainScene: BaseScene
{
    protected override void OnInitialize()
    {
        AddGameObject(new Editor());
        AddGameObject(new MainGameObject());
    }

    protected override void OnUpdate(float deltaTime)
    {
    }

    protected override void OnDispose()
    {
    }
}