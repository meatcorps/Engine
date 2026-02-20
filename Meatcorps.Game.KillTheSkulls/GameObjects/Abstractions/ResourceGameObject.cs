using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Game.KillTheSkulls.Data;
using Meatcorps.Game.KillTheSkulls.GameEnums;
using Meatcorps.Game.KillTheSkulls.Scenes;

namespace Meatcorps.Game.KillTheSkulls.GameObjects.Abstractions;

public abstract class ResourceGameObject : BaseGameObject
{
    protected Texture2DItem<GameSprites> Sprites { get; private set; } = null!;
    protected TextManager<DefaultFont> Fonts { get; private set; } = null!;
    protected LevelData LevelData { get; private set; } = null!;
    protected SoundFxManager<GameSounds> Sounds { get; private set; } = null!;
    public MusicManager<GameMusic> Music { get; private set; } = null!;
    public IArcadePointsMutator PointMutator { get; private set; } = null!;

    protected bool DemoMode { get; private set; }

    protected override void OnInitialize()
    {
        Sprites = GlobalObjectManager.ObjectManager.Get<Texture2DItem<GameSprites>>()!;
        Fonts = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!;
        Sounds = GlobalObjectManager.ObjectManager.Get<SoundFxManager<GameSounds>>()!;
        Music = GlobalObjectManager.ObjectManager.Get<MusicManager<GameMusic>>()!;
        PointMutator = GlobalObjectManager.ObjectManager.Get<IArcadePointsMutator>()!;

        if (Scene is LevelScene levelScene)
        {
            LevelData = Scene.SceneObjectManager.Get<LevelData>()!;
            DemoMode = levelScene.DemoMode;
        }
    }
}