using Meatcorps.Engine.Collision.Data;
using Meatcorps.Engine.Collision.Enums;
using Meatcorps.Engine.Collision.Extensions;
using Meatcorps.Engine.Collision.Interfaces;
using Meatcorps.Engine.Collision.Providers.Bodies;
using Meatcorps.Engine.Collision.Utilities;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Game.Pacman.GameEnums;
using Meatcorps.Game.Pacman.GameObjects.Abstractions;
using Raylib_cs;

namespace Meatcorps.Game.Pacman.GameObjects;

public class Collectable: ResourceGameObject, ICollisionEventsFiltered
{
    private readonly PointInt _position;
    private Body _body;
    private bool _isCollected;
    private RandomEnum<GameSounds> _randomSound = new RandomEnum<GameSounds>()
        .Add(GameSounds.Nlpakem1, 25)
        .Add(GameSounds.Nlpakem2, 25)
        .Add(GameSounds.Nlpakem3, 25)
        .Add(GameSounds.Nlpakem4, 25)
        .Add(GameSounds.Nlpakem5, 25);
    
    public Collectable(PointInt position)
    {
        _position = position;
    }

    protected override void OnInitialize()
    {
        Enabled = false;
        base.OnInitialize();
        Layer = 3;
        var rect = LevelData.ToWorldRectangle(_position).ToRectF();
        rect.Inflate(-5,-5);
        _body = WorldService.RegisterRectFBodySensor(this, rect)
            .SetType(BodyType.Kinematic)
            .SetLayer(LayerBits.Bit(CollisionLayer.Items))
            .SetMask(LayerBits.MaskOf(CollisionLayer.PacMan));
        LevelData.CollectibleCount++;
        Scene.AddGameObject(new Drone(_body.Position, GameSprites.CollectibleMeat, () => Enabled = true, 6000 + _position.X * 10));
    }

    protected override void OnUpdate(float deltaTime)
    {
    }

    protected override void OnDraw()
    {
        var worldPosition = LevelData.ToWorldPosition(_position);
        //if (_isCollected)
        //    Sprites.Draw(GameSprites.CollectibleMeat, worldPosition, Raylib.ColorAlpha(Color.Black, 0.5f));
        //else
        if (!_isCollected)
            Sprites.Draw(LevelData.DutchMode ? GameSprites.CollectibleCheese : GameSprites.CollectibleMeat, worldPosition, Raylib.ColorAlpha(Color.White, 1f));
    }

    protected override void OnDispose()
    {
        _body.Dispose();
    }


    public void OnContact(ContactPhase phase, in ContactPair pair, in ContactManifold manifold)
    {
    }

    public void OnTrigger(ContactPhase phase, in ContactPair pair)
    {
        if (_isCollected)
            return;
        _body.IsAwake = false;
        _isCollected = true;
        LevelData.CollectiblesGone++;
        CameraManager.Shake(0.05f, 10f);
        //Console.WriteLine(LevelData.CollectiblesGone + " / " + LevelData.CollectibleCount + " TODO " + (LevelData.CollectibleCount - LevelData.CollectiblesGone));
        if (!DemoMode)
            if (LevelData.DutchMode)
            {
                var sound = _randomSound.Get();
                Sounds.Play(sound);
            }
            else 
                Sounds.Play(GameSounds.Meatonground);
        _body.Dispose();
    }
}