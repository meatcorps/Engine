using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Game.HorrorJackpot.Data;
using Meatcorps.Game.HorrorJackpot.GameEnums;
using Meatcorps.Game.HorrorJackpot.Resources;
using Meatcorps.Game.HorrorJackpot.Scenes;

namespace Meatcorps.Game.HorrorJackpot.GameObjects.Abstractions;

public abstract class ResourceGameObject : BaseGameObject
{
    protected Texture2DItem<GameSprites> Sprites { get; private set; }
    protected TextManager<GameFonts> Fonts { get; private set; }
    protected LevelData LevelData { get; private set; }
    public SoundFxManager<GameSounds> Sounds { get; private set; }
    public MusicManager<GameMusic> Music { get; private set; }
    public IArcadePointsMutator PointMutator { get; private set; }

    protected override void OnInitialize()
    {
        Sprites = GlobalObjectManager.ObjectManager.Get<Texture2DItem<GameSprites>>()!;
        Fonts = GlobalObjectManager.ObjectManager.Get<TextManager<GameFonts>>()!;
        Sounds = GlobalObjectManager.ObjectManager.Get<SoundFxManager<GameSounds>>()!;
        Music = GlobalObjectManager.ObjectManager.Get<MusicManager<GameMusic>>()!;
        PointMutator = GlobalObjectManager.ObjectManager.Get<IArcadePointsMutator>()!;
        
        if (Scene is LevelScene)
            LevelData = Scene.SceneObjectManager.Get<LevelData>()!;
        else
            LevelData = new();
    }
}