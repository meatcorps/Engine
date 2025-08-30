using System.Numerics;
using Meatcorps.Engine.Collision.Data;
using Meatcorps.Engine.Collision.Enums;
using Meatcorps.Engine.Collision.Extensions;
using Meatcorps.Engine.Collision.Interfaces;
using Meatcorps.Engine.Collision.Providers.Bodies;
using Meatcorps.Engine.Collision.Utilities;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Game.Pacman.GameEnums;
using Meatcorps.Game.Pacman.GameObjects.Abstractions;
using Raylib_cs;

namespace Meatcorps.Game.Pacman.GameObjects;

public class PowerPellet: ResourceGameObject, ICollisionEventsFiltered
{
    private readonly PointInt _position;
    private Body _body;
    
    public PowerPellet(PointInt position)
    {
        _position = position;
    }

    protected override void OnInitialize()
    {
        Enabled = false;
        base.OnInitialize();
        Layer = 3;
        var rect = LevelData.ToWorldRectangle(_position).ToRectF();
        rect.Inflate(-3,-3);
        _body = WorldService.RegisterRectFBodySensor(this, rect)
            .SetType(BodyType.Kinematic)
            .SetLayer(LayerBits.Bit(CollisionLayer.Items))
            .SetMask(LayerBits.MaskOf(CollisionLayer.PacMan));
        Scene.AddGameObject(new Drone(_body.Position, GameSprites.SuperPacmanPowerUp1, () => Enabled = true, 6000));
    }

    protected override void OnUpdate(float deltaTime)
    {
    }

    protected override void OnDraw()
    {
        Sprites.Draw(GameSprites.SuperPacmanPowerUp1, LevelData.ToWorldPosition(_position), Color.White);
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
        LevelData.GhostScared = true;
        LevelData.GhostScaredResetTimer = true;
        CameraManager.Shake(0.6f, 4f);
        Sounds.Play(GameSounds.Warning);
        MessageUI.Show("GET THEM!!! KILL AND ATTACK!");
        Scene.RemoveGameObject(this);
    }
}