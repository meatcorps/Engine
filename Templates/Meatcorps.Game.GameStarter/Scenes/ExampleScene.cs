using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Text;
using Meatcorps.Game.GameStarter.GameObjects;

namespace Meatcorps.Game.GameStarter.Scenes;

public class ExampleScene: BaseScene
{
    protected override void OnInitialize()
    {
        AddGameObject(new UIMessageEmitter(
            TextStyle.Create(
                GlobalObjectManager.ObjectManager.Get<IDefaultFont>()!.GetFont(),
                16)));
        AddGameObject(new CameraControllerGameObject(GlobalObjectManager.ObjectManager.Get<ICamera>()));
        AddGameObject(new ExampleGameObject());
    }

    protected override void OnUpdate(float deltaTime)
    {
    }

    protected override void OnDispose()
    {
    }
}