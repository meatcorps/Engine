using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Game.HorrorJackpot.GameEnums;

namespace Meatcorps.Game.HorrorJackpot.GameObjects.Abstractions;

public abstract class ResourceGameObject : BaseGameObject
{
    protected Texture2DItem<GameSprites> Sprites { get; private set; } = null!;
    protected TextManager<GameFonts> Fonts { get; private set; } = null!;
    public SoundFxManager<GameSounds> Sounds { get; private set; } = null!;
    public MusicManager<GameMusic> Music { get; private set; } = null!;
    public IArcadePointsMutator PointMutator { get; private set; } = null!;

    protected override void OnInitialize()
    {
        Sprites = GlobalObjectManager.ObjectManager.Get<Texture2DItem<GameSprites>>()!;
        Fonts = GlobalObjectManager.ObjectManager.Get<TextManager<GameFonts>>()!;
        Sounds = GlobalObjectManager.ObjectManager.Get<SoundFxManager<GameSounds>>()!;
        Music = GlobalObjectManager.ObjectManager.Get<MusicManager<GameMusic>>()!;
        PointMutator = GlobalObjectManager.ObjectManager.Get<IArcadePointsMutator>()!;
    }
}