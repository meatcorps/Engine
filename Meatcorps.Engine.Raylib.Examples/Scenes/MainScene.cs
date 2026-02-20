using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Examples.GameObjects;

namespace Meatcorps.Engine.Raylib.Examples.Scenes;

public class MainScene : BaseScene
{
    protected override void OnInitialize()
    {
        //AddGameObject(new Fireworks());
        AddGameObject(new AsciiScriptPlayground());
        //AddGameObject(new InputLocalSinglePlayerExample());

        //AddGameObject(new ImGuiTest());

        //AddGameObject(new RawControllerTest());
    }

    protected override void OnUpdate(float deltaTime)
    {
    }

    protected override void OnDispose()
    {
    }
}