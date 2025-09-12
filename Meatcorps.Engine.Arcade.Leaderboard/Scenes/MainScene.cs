using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Text;
using Meatcorps.Engine.Session;
using Meatcorps.Engine.Arcade.Leaderboard.GameEnums;
using Meatcorps.Engine.Arcade.Leaderboard.GameObjects;
using Raylib_cs;

namespace Meatcorps.Engine.Arcade.Leaderboard.Scenes;

public class MainScene : BaseScene
{
    private Font _font;
    private UIMessageEmitter _uiMessage;
    private SessionService<GameSessionData, GamePlayerData> _sessionService;

    protected override void OnInitialize()
    {
        _uiMessage = new UIMessageEmitter(TextKitStyles.HudDefault(_font));
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