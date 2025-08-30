using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Game.Snake.Data;
using Meatcorps.Game.Snake.Resources;
using Meatcorps.Game.Snake.Scenes;

namespace Meatcorps.Game.Snake.GameObjects.Abstractions;

public abstract class SnakeGameObject: BaseGameObject
{
    protected Texture2DItem<SnakeSprites> Sprites { get; private set; }
    protected TextManager<DefaultFont> Fonts { get; private set; }
    protected LevelData LevelData { get; private set; }
    protected SoundFxManager<SnakeSounds> Sounds { get; private set; }
    public MusicManager<SnakeMusic> Music { get; private set; }
    public IArcadePointsMutator PointMutator { get; private set; }

    protected override void OnInitialize()
    {
        Sprites = GlobalObjectManager.ObjectManager.Get<Texture2DItem<SnakeSprites>>()!;
        Fonts = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!;
        Sounds = GlobalObjectManager.ObjectManager.Get<SoundFxManager<SnakeSounds>>()!;
        Music = GlobalObjectManager.ObjectManager.Get<MusicManager<SnakeMusic>>()!;
        PointMutator = GlobalObjectManager.ObjectManager.Get<IArcadePointsMutator>()!;
        
        if (Scene is LevelScene)
            LevelData = Scene.SceneObjectManager.Get<LevelData>()!;
        else
            LevelData = new();
    }
}