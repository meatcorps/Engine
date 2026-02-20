using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Enums;
using Meatcorps.Engine.Core.Interfaces.Trackers;
using Meatcorps.Engine.RayLib.Abstractions;

namespace Meatcorps.Engine.Arcade.RayLib.Services;

public class ArcadeGameStateTracker: ISceneSwitchTracker
{
    private readonly ArcadeGame _game;
    private Type? _introScene;
    
    public ArcadeGameStateTracker(ArcadeGame game)
    {
        _game = game;
    }

    public void SetIntroScene<T>() where T : BaseScene
    {
        _introScene = typeof(T);
    }
    
    public void OnActiveSceneSwitch(object scene)
    {
        if (_introScene == null)
            throw new ArgumentNullException(nameof(scene), "Is not called");
        
        if (scene.GetType() == _introScene)
            _game.State = GameState.Idle;
    }
}