using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.GameObjects;
using Meatcorps.Game.KillTheSkulls.Data;
using Meatcorps.Game.KillTheSkulls.GameEnums;
using Meatcorps.Game.KillTheSkulls.GameObjects.Abstractions;
using Raylib_cs;

namespace Meatcorps.Game.KillTheSkulls.GameObjects;

public class PipeOverlay : ResourceGameObject
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Layer = 8;
    }

    protected override void OnUpdate(float deltaTime)
    {
       //
    }

    protected override void OnDraw()
    {
        Sprites.Draw(GameSprites.PipeOverlay, new Vector2(0, 240), Color.White);
    }

    protected override void OnDispose()
    {
    }
}