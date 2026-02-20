using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Collision.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Game.CyberMaze.Data;
using Meatcorps.Game.CyberMaze.GameEnums;
using Meatcorps.Game.CyberMaze.Scenes;

namespace Meatcorps.Game.CyberMaze.GameObjects.Abstractions;

public abstract class ResourceGameObject : BaseGameObject
{
    protected Texture2DItem<GameSprites> Sprites { get; private set; } = null!;
    protected TextManager<DefaultFont> Fonts { get; private set; } = null!;
    protected LevelData LevelData { get; private set; } = null!;
    protected SoundFxManager<GameSounds> Sounds { get; private set; } = null!;
    protected MusicManager<GameMusic> Music { get; private set; } = null!;
    protected IArcadePointsMutator PointMutator { get; private set; } = null!;
    protected WorldService  WorldService { get; private set; } = null!;
    protected CameraControllerGameObject CameraManager { get; private set; } = null!;
    protected UIMessageEmitter MessageUI { get; private set; } = null!;
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