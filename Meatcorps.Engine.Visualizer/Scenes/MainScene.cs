using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Text;
using Meatcorps.Engine.Visualizer.GameObjects;

namespace Meatcorps.Engine.Visualizer.Scenes;

public class MainScene: BaseScene
{
    protected override void OnInitialize()
    {
        AddGameObject(new CameraControllerGameObject(GlobalObjectManager.ObjectManager.Get<ICamera>()));
        AddGameObject(new UIMessageEmitter(
            TextKitStyles.HudDefault(GlobalObjectManager.ObjectManager.Get<IDefaultFont>()!.GetFont())));
        AddGameObject(new Toolbox());
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