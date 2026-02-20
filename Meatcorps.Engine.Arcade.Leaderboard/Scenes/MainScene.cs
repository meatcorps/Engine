using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Text;
using Meatcorps.Engine.Arcade.Leaderboard.GameObjects;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.Arcade.Leaderboard.Scenes;

public class MainScene : BaseScene
{
    private UIMessageEmitter _uiMessage = null!;

    protected override void OnInitialize()
    {
        _uiMessage =
            new UIMessageEmitter(
                TextKitStyles.HudDefault(GlobalObjectManager.ObjectManager.Get<IDefaultFont>()!.GetFont()));
        AddGameObject(_uiMessage);
        AddGameObject(new MainGameObject());
    }

    protected override void OnUpdate(float deltaTime)
    {
    }

    protected override void OnDispose()
    {
    }
}