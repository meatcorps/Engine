using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.Session;
using Meatcorps.Engine.Session.Data;
using Meatcorps.Game.Snake.Data;
using Meatcorps.Game.Snake.GameObjects.UI;
using Meatcorps.Game.Snake.Resources;

namespace Meatcorps.Game.Snake.Scenes;

public class EndScene : BaseScene
{
    private TimerOn _timer = new(30000);

    public int TimeLeft => (int)(_timer.TimeRemaining / 1000);
    
    protected override void OnInitialize()
    {
        var totalPlayers = GlobalObjectManager.ObjectManager.Get<SessionService<SnakeSessionData, SnakePlayerData>>()!.CurrentSession.TotalPlayers;
        var renderer = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()!;
        GlobalObjectManager.ObjectManager.Get<MusicManager<SnakeMusic>>()!.Play(SnakeMusic.IntroOutro);
        AddGameObject(new EndGameGameObject());
        for (var i = 0; i < totalPlayers; i++)
        {
            var width = renderer.RenderWidth / totalPlayers;
            var bounds = new Rect(i * width, 16, width, renderer.RenderHeight - 32);
            AddGameObject(new FinalScoreCalculator(bounds, i + 1));
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        _timer.Update(true, deltaTime);
        if (_timer.Output)
            GameHost.SwitchScene(new IntroScene());
    }

    protected override void OnDispose()
    {
    }
}