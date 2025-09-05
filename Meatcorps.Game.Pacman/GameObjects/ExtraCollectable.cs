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

public class ExtraCollectable: ResourceGameObject, ICollisionEventsFiltered
{
    private readonly PointInt _position;
    private Body _body;
    private bool _isCollected;

    private RandomEnum<GameSounds> _randomSound = new RandomEnum<GameSounds>()
        .Add(GameSounds.Nlpakemhoog1, 25)
        .Add(GameSounds.Nlpakem2hoog, 25)
        .Add(GameSounds.Nlpakem3hoog, 25)
        .Add(GameSounds.Nlpakem4hoog, 25)
        .Add(GameSounds.Nlpakem5hoog, 25);
    
    public ExtraCollectable(PointInt position)
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
        Scene.AddGameObject(new Drone(_body.Position, GameSprites.SuperPacmanPowerUp2, () => Enabled = true, _position.X * 10));
    }

    protected override void OnUpdate(float deltaTime)
    {
    }

    protected override void OnDraw()
    {
        var worldPosition = LevelData.ToWorldPosition(_position);
        if (!_isCollected)
            Sprites.Draw(GameSprites.SuperPacmanPowerUp2, worldPosition, Raylib.ColorAlpha(Color.White, 1f));
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
        CameraManager.Shake(0.8f, 4f);
        
        if (!DemoMode)
            if (LevelData.DutchMode)
                Sounds.Play(_randomSound.Get());
            else 
                Sounds.Play(GameSounds.PowerUpScore);
        
        _body.Dispose();
    }
}