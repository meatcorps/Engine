using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Game.GameStarter.GameEnums;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Meatcorps.Game.GameStarter.GameObjects.Abstractions;

public abstract class ResourceGameObject : BaseGameObject
{
    protected Texture2DItem<GameSprites> Sprites { get; private set; } = null!;
    protected TextManager<DefaultFont> Fonts { get; private set; } = null!;
    protected SoundFxManager<GameSounds> Sounds { get; private set; } = null!;
    protected MusicManager<GameMusic> Music { get; private set; } = null!;
    protected CameraControllerGameObject CameraManager { get; private set; } = null!;
    protected UIMessageEmitter MessageUI { get; private set; } = null!;
    
    protected override void OnInitialize()
    {
        Sprites = GlobalObjectManager.ObjectManager.Get<Texture2DItem<GameSprites>>()!;
        Fonts = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!;
        Sounds = GlobalObjectManager.ObjectManager.Get<SoundFxManager<GameSounds>>()!;
        Music = GlobalObjectManager.ObjectManager.Get<MusicManager<GameMusic>>()!;
        CameraManager = Scene.GetGameObject<CameraControllerGameObject>()!;
        MessageUI = Scene.GetGameObject<UIMessageEmitter>()!;
    }

}