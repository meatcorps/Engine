using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Collision.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Game.Pacman.Data;
using Meatcorps.Game.Pacman.GameEnums;
using Meatcorps.Game.Pacman.Resources;
using Meatcorps.Game.Pacman.Scenes;

namespace Meatcorps.Game.Pacman.GameObjects.Abstractions;

public abstract class ResourceGameObject : BaseGameObject
{
    protected Texture2DItem<GameSprites> Sprites { get; private set; }
    protected TextManager<DefaultFont> Fonts { get; private set; }
    protected LevelData LevelData { get; private set; }
    protected SoundFxManager<GameSounds> Sounds { get; private set; }
    protected MusicManager<GameMusic> Music { get; private set; }
    protected IArcadePointsMutator PointMutator { get; private set; }
    protected WorldService  WorldService { get; private set; }
    protected CameraControllerGameObject CameraManager { get; private set; }
    protected UIMessageEmitter MessageUI { get; private set; }
    protected bool DemoMode { get; private set; }
    
    protected override void OnInitialize()
    {
        WorldService = Scene.SceneObjectManager.Get<WorldService>()!;
        Sprites = GlobalObjectManager.ObjectManager.Get<Texture2DItem<GameSprites>>()!;
        Fonts = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!;
        Sounds = GlobalObjectManager.ObjectManager.Get<SoundFxManager<GameSounds>>()!;
        Music = GlobalObjectManager.ObjectManager.Get<MusicManager<GameMusic>>()!;
        PointMutator = GlobalObjectManager.ObjectManager.Get<IArcadePointsMutator>()!;
        CameraManager = Scene.GetGameObject<CameraControllerGameObject>()!;
        MessageUI = Scene.GetGameObject<UIMessageEmitter>()!;
        if (Scene is LevelScene levelScene)
        {
            LevelData = Scene.SceneObjectManager.Get<LevelData>()!;
            DemoMode = levelScene.DemoMode;
        }
        else
            LevelData = new();
    }

}