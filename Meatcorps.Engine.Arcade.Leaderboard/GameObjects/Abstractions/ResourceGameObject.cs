using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.Arcade.Leaderboard.Data;
using Meatcorps.Engine.Arcade.Leaderboard.GameEnums;
using Meatcorps.Engine.Arcade.Leaderboard.Resources;
using Meatcorps.Engine.Arcade.Leaderboard.Scenes;

namespace Meatcorps.Engine.Arcade.Leaderboard.GameObjects.Abstractions;

public abstract class ResourceGameObject : BaseGameObject
{
    protected Texture2DItem<GameSprites> Sprites { get; private set; }
    protected TextManager<DefaultFont> Fonts { get; private set; }

    protected override void OnInitialize()
    {
        Sprites = GlobalObjectManager.ObjectManager.Get<Texture2DItem<GameSprites>>()!;
        Fonts = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!;
    }
}