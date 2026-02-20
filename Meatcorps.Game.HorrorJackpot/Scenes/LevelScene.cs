using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.RayLib.Text;
using Meatcorps.Engine.Session;
using Meatcorps.Game.HorrorJackpot.GameEnums;
using Meatcorps.Game.HorrorJackpot.GameObjects;
using Raylib_cs;

namespace Meatcorps.Game.HorrorJackpot.Scenes;

public class LevelScene : BaseScene
{
    private Font _font;
    private UIMessageEmitter _uiMessage = null!;
    private PlayerInputRouter<GameInput> _controller = null!;
    private MusicManager<GameMusic> _musicManager = null!;
    private SessionService<GameSessionData, GamePlayerData> _sessionService = null!;

    protected override void OnInitialize()
    {
        _font = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!.GetFont();
        _uiMessage = new UIMessageEmitter(TextKitStyles.HudDefault(_font));

        _controller = GlobalObjectManager.ObjectManager.Get<PlayerInputRouter<GameInput>>()!;
        _sessionService = GlobalObjectManager.ObjectManager.Get<SessionService<GameSessionData, GamePlayerData>>()!;
        AddGameObject(_uiMessage);
        AddGameObject(new MainGameObject());

        _controller.GetState(1, GameInput.Action).Animation = new BlinkAnimation(250);
        
        _musicManager = GlobalObjectManager.ObjectManager.Get<MusicManager<GameMusic>>()!;
        _musicManager.Play(GameMusic.IntroOutro);
        _sessionService.StartSession();
    }

    protected override void OnUpdate(float deltaTime)
    {
        
    }

    protected override void OnDispose()
    {
        _controller.GetState(1, GameInput.Action).Animation = null;
        _controller.GetState(2, GameInput.Action).Animation = null;
    }
}