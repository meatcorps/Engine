using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.UI.GuiComponent.GuiSettings;
using Meatcorps.Engine.Session;
using Meatcorps.Game.CyberMaze.GameEnums;
using Meatcorps.Game.CyberMaze.GameObjects.UI;

namespace Meatcorps.Game.CyberMaze.Scenes;

public class EndScene : BaseScene
{
    private TimerOn _timer = new(16000);
    private PlayerInputRouter<GameInput> _controller;
    private DefaultGuiSettings<GameInput, GameSounds>? _guiSettings = null;

    public int TimeLeft => (int)(_timer.TimeRemaining / 1000);

    protected override void OnInitialize()
    {
        var settings = GlobalObjectManager.ObjectManager.Get<IGuiSettings>();
        if (settings is DefaultGuiSettings<GameInput, GameSounds> guiSettings)
            _guiSettings = guiSettings;
        
        var totalPlayers = GlobalObjectManager.ObjectManager.Get<SessionService<GameSessionData, GamePlayerData>>()!
            .CurrentSession.TotalPlayers;
        var renderer = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()!;
        GlobalObjectManager.ObjectManager.Get<MusicManager<GameMusic>>()!.Play(GameMusic.IntroOutro);
        AddGameObject(new EndGameGameObject());
        for (var i = 0; i < totalPlayers; i++)
        {
            var width = renderer.RenderWidth / totalPlayers;
            var bounds = new Rect(i * width, 16, width, renderer.RenderHeight - 32);
            AddGameObject(new FinalScoreCalculator(bounds, i + 1));
        }
        _controller = GlobalObjectManager.ObjectManager.Get<PlayerInputRouter<GameInput>>()!;
        _controller.GetState(1, GameInput.Action).Animation = new BlinkAnimation(250);
        _controller.GetState(2, GameInput.Action).Animation = new BlinkAnimation(250);
    }

    protected override void OnPreUpdate(float deltaTime)
    {
        
        _guiSettings?.Update(deltaTime);
        base.OnPreUpdate(deltaTime);
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (_guiSettings is not null)
        {
            if (_guiSettings.IsBackPressed)
            {
                GameHost.SwitchScene(new IntroScene());
            }
        }
        
        _timer.Update(true, deltaTime);
        if (_timer.Output || (TimeLeft < 12 && (_controller.GetState(1, GameInput.Action).IsPressed || _controller.GetState(2, GameInput.Action).IsPressed)))
            GameHost.SwitchScene(new IntroScene());
    }

    protected override void OnDispose()
    {
    }
}