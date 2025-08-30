using System.Numerics;
using Meatcorps.Engine.Collision.Data;
using Meatcorps.Engine.Collision.Enums;
using Meatcorps.Engine.Collision.Extensions;
using Meatcorps.Engine.Collision.Interfaces;
using Meatcorps.Engine.Collision.Services;
using Meatcorps.Engine.Collision.Utilities;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Particles;
using Meatcorps.Engine.RayLib.PostProcessing;
using Meatcorps.Game.Pacman.Data;
using Meatcorps.Game.Pacman.GameEnums;
using Meatcorps.Game.Pacman.GameObjects.Abstractions;
using Meatcorps.Game.Pacman.GameObjects.GhostManagers;
using Meatcorps.Game.Pacman.GameObjects.UI;
using Meatcorps.Game.Pacman.Scenes;
using Raylib_cs;

namespace Meatcorps.Game.Pacman.GameObjects;

public class PacMan: BasePlayer, ICollisionEventsFiltered
{
    private bool _collided;
    private Vector2 _lastVelocity;
    private BufferedDirection _bufferedDirection;
    private FixedTimer _animationTimer = new(300);
    private ParticleSystemBuilder _bloodParticle;
    private ParticleSystemBuilder _smokeParticle;
    private ParticleSystemBuilder _scoreParticle;
    private ParticleSystemBuilder _explosionParticle;
    private TimerOn _diedTimer1 = new(3000);
    private TimerOn _diedTimer2 = new(3000);
    private bool _diedSecondFase;
    private bool _died;
    private bool _renderPacman;
    private EdgeDetector _GhostScaredEdgeDetector;
    
    public PacMan(Player _player) : base(_player)
    {
        _bufferedDirection = new BufferedDirection(500);
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        LevelData.Players.Add(Player);
        _GhostScaredEdgeDetector = new EdgeDetector();
        _bloodParticle = Particles.BloodParticle.GenerateParticleSystem();
        _smokeParticle = Particles.SmokeParticle.GenerateParticleSystem(Sprites);
        _scoreParticle = Particles.ScoreParticle.GenerateParticleSystem(Color.White, Fonts.GetFont());
        _explosionParticle = Particles.ExplosionParticle.GenerateParticleSystem(Sprites, Sounds);
        Layer = 3;
        LevelData.TargetPacman = Player;
        Position = Player.Body.Position;
 
        if (DemoMode)
        {
            Player.Ui.Enabled = false;
            Player.Body.SetMask(LayerBits.MaskOf(CollisionLayer.Wall, CollisionLayer.Items, CollisionLayer.PacMan));
        }

        Enabled = false;
    }

    protected override void OnEnabled()
    {
        Player.Ui.Enabled = true;
        base.OnEnabled();
    }

    protected override void OnUpdate(float deltaTime)
    {
        _GhostScaredEdgeDetector.Update(LevelData.GhostScared);
        if (!DemoMode)
            if (LevelData.GhostScared)
            {
                Music.Play(GameMusic.LevelHeavy);
            }
            else
            {
                Music.Play(GameMusic.LevelAmbient);
            }
        _diedTimer1.Update(_died, deltaTime);
        _diedTimer2.Update(_diedTimer1.Output, deltaTime);
        _scoreParticle.Update(deltaTime);
        _bloodParticle.Update(deltaTime);
        _smokeParticle.Update(deltaTime);
        _animationTimer.Update(deltaTime);
        _explosionParticle.Update(deltaTime);
        var speed = LevelData.Speed;

        if (!_diedTimer1.Output && _died)
        {
            if (!DemoMode)
                Music.Pause();
            
            _explosionParticle.Emit(4, Player.Body.BoundingBox.Center);
            CameraManager.Shake(1.5f);
        }

        if (_diedTimer1.Output && !_diedSecondFase)
        {
            Scene.AddGameObject(new ScreenFlash(Color.Red, 6000));
            MessageUI.Show("WELL, SURPRISE YOU DIED!");
            _diedSecondFase = true;
        }
        
        if (_diedTimer2.Output)
        {
            EndPlayer();
            Enabled = false;
        }
        
        if (LevelData.FreezePlayersAndGhosts)
        {
            Player.Body.Velocity = Vector2.Zero;
            return;
        }

        var raw = Vector2.Zero;
        if (!DemoMode)
        {
            raw = Controller.GetAxis(Player.PlayerId);
        }
        else
        {
            raw = Player.Body.Velocity.IsEqualsSafe(Vector2.Zero) ? new Vector2(Raylib.GetRandomValue(-1, 1), Raylib.GetRandomValue(-1, 1)) : _bufferedDirection.Direction;
        }

        _bufferedDirection.Update(raw, deltaTime);
        
        if (_bufferedDirection.IsDirectionChangedAndIsNotZero(Player.Body.Velocity, speed, out var direction))
            GridMovement.TryMove(Player.Body, direction, ref _lastVelocity, deltaTime, LayerBits.MaskOf(CollisionLayer.Wall), GetRoundingRatio(speed));

        if (!Player.Body.Velocity.IsEqualsSafe(Vector2.Zero))
            Player.Body.Velocity = Player.Body.Velocity.NormalizedCopy() * speed;
        
        Player.Body.SetMaxSpeed(250);
        Player.Body.Position = Player.Body.Position.Warp(LevelData.LevelWidth * (LevelData.GridSize), LevelData.LevelHeight * (LevelData.GridSize));
        
        _collided = false;
        
        if (LevelData.GhostScared)
            _smokeParticle.Emit(1, Player.Body.BoundingBox.Center - Player.Body.Velocity.NormalizedCopy() * 8);

        if (LevelData.CollectiblesGone == LevelData.CollectibleCount)
        {
            AllCollectiblesCollected();
        }
    }

    private float GetRoundingRatio(float speed)
    {
        if (speed < 50)
            return 1f;
        if (speed < 100)
            return 4f;
        
        return 4;
    }

    protected override void OnDraw()
    {
        var position = Player.Body.Position;
        var facing = _lastVelocity.NormalizedCopy().ToPointInt();
        if (_died)
        {
            if (!_diedSecondFase)
                Sprites.Draw(GameSprites.PacmanDown1, position, Color.Black);
        }
        else
        {
            if (!LevelData.GhostScared)
            {
                if (facing == new PointInt(0, 1) || facing == new PointInt(0, 0))
                    Sprites.DrawAnimationWithNormal(GameSprites.PacmanDownAnimation, _animationTimer.NormalizedElapsed,
                        position);
                if (facing == new PointInt(0, -1))
                    Sprites.DrawAnimationWithNormal(GameSprites.PacmanUpAnimation, _animationTimer.NormalizedElapsed,
                        position);
                if (facing == new PointInt(1, 0))
                    Sprites.DrawAnimationWithNormal(GameSprites.PacmanRightAnimation, _animationTimer.NormalizedElapsed,
                        position);
                if (facing == new PointInt(-1, 0))
                    Sprites.DrawAnimationWithNormal(GameSprites.PacmanLeftAnimation, _animationTimer.NormalizedElapsed,
                        position);
            }
            else
            {
                if (facing == new PointInt(0, 1) || facing == new PointInt(0, 0))
                    Sprites.DrawAnimationWithNormal(GameSprites.SuperPacmanDownAnimation,
                        _animationTimer.NormalizedElapsed, position);
                if (facing == new PointInt(0, -1))
                    Sprites.DrawAnimationWithNormal(GameSprites.SuperPacmanUpAnimation,
                        _animationTimer.NormalizedElapsed, position);
                if (facing == new PointInt(1, 0))
                    Sprites.DrawAnimationWithNormal(GameSprites.SuperPacmanRightAnimation,
                        _animationTimer.NormalizedElapsed, position);
                if (facing == new PointInt(-1, 0))
                    Sprites.DrawAnimationWithNormal(GameSprites.SuperPacmanLeftAnimation,
                        _animationTimer.NormalizedElapsed, position);

                Sprites.DrawAnimationWithNormal(GameSprites.SuperPacmanEffectAnimation,
                    _animationTimer.NormalizedElapsed, position, Raylib.ColorAlpha(Color.White, 0.5f));
            }
        }

        _explosionParticle.Draw();
        _smokeParticle.Draw();
        _bloodParticle.Draw();
        _scoreParticle.Draw();
    }

    protected override void OnDispose()
    {
        
    }

    protected override void PlayerLost()
    {
        EndPlayer();
    }

    public static void Setup(WorldService world, Player player, Vector2 position)
    {
        player.PacMan = new PacMan(player);
        player.Body = world.RegisterRectFBody(player.PacMan, new RectF(position, new SizeF(16, 16)));
        player.Body.SetLayer(LayerBits.Bit(CollisionLayer.PacMan));
        player.Body.SetMask(LayerBits.MaskOf(CollisionLayer.Wall, CollisionLayer.Items, CollisionLayer.Ghost, CollisionLayer.PacMan));
        player.Ui = new PlayerUI(player);
    }

    public void OnContact(ContactPhase phase, in ContactPair pair, in ContactManifold manifold)
    {

        
    }

    private void AddScore(int amount)
    {
        _scoreParticle.KillAll();
        _scoreParticle.Emit(1, Player.Body.BoundingBox.Center + new Vector2(0, -8) + Player.Body.Velocity.NormalizedCopy() * 8, null, "+" + amount.ToString());
        Player.Score += amount;
    }

    private void AllCollectiblesCollected()
    {
        if (!DemoMode)
            Music.Pause();

        Sounds.Play(GameSounds.PowerUpScore);

        Scene.AddGameObject(new ScreenFlash(Color.White));
        MessageUI.Show("All Collectibles Collected! NEXT!");
        LevelData.FreezePlayersAndGhosts = true;
        Scene.AddGameObject(new Drone(Player.Body.Position, GameSprites.PacmanDown1, () => Enabled = false, 1500, true, EndGame));

        PickupGhosts();
    }

    private void PickupGhosts()
    {
        foreach (var ghost in LevelData.Ghosts)
        {
            switch (ghost.Type)
            {
                case GhostType.Blinky:
                    Scene.AddGameObject(new Drone(ghost.Body.Position, GameSprites.PacmanDown1, () => ghost.Enabled = false, 1000, true));
                    break;
                case GhostType.Pinky:
                    Scene.AddGameObject(new Drone(ghost.Body.Position, GameSprites.GhostPinky1, () => ghost.Enabled = false, 1100, true));
                    break;
                case GhostType.Inky:
                    Scene.AddGameObject(new Drone(ghost.Body.Position, GameSprites.GhostInky1, () => ghost.Enabled = false, 1200, true));
                    break;
                case GhostType.Clyde:
                    Scene.AddGameObject(new Drone(ghost.Body.Position, GameSprites.GhostClyde1, () => ghost.Enabled = false, 1300, true));
                    break;
            }
        }
    }
    
    public void OnTrigger(ContactPhase phase, in ContactPair pair)
    {
        if (pair.TryGetOwner<Collectable>(out var _))
        {
            Player.AddValue(GamePlayerData.Eaten);
            AddScore(100);
            _bloodParticle.Emit(4, Player.Body.BoundingBox.Center + Player.Body.Velocity.NormalizedCopy() * 8);
        }
        if (pair.TryGetOwner<ExtraCollectable>(out var _))
        {
            Player.AddValue(GamePlayerData.Eaten);
            AddScore(1000);
            _smokeParticle.Emit(4, Player.Body.BoundingBox.Center + Player.Body.Velocity.NormalizedCopy() * 8);
        }

        if (pair.TryGetOwner<PowerPellet>(out var _))
        {
            AddScore(200);
            Player.AddValue(GamePlayerData.SuperPacMan);
        }

        if (pair.TryGetOwner<Ghost>(out var ghost))
        {
            // Add this after testing. For now god mode :)
            if (!LevelData.GhostScared)
            {
                LevelData.FreezePlayersAndGhosts = true;
                _explosionParticle.Emit(4, Player.Body.BoundingBox.Center);
                PickupGhosts();
                foreach (var player in LevelData.Players)
                {
                    if (player.PlayerId != Player.PlayerId)
                    {
                        Scene.AddGameObject(new Drone(player.Body.Position, GameSprites.PacmanDown1, () => Enabled = false, 1500, true, EndGame));
                    }
                }

                CameraManager.SetZoom(0.5f);
                Position = Player.Body.Position;
                CameraManager.SetBounds(new Rect(0, 0, LevelData.LevelWidth * (LevelData.GridSize), LevelData.LevelHeight * (LevelData.GridSize)));
                CameraManager.Follow(this);
                Player.Body.Dispose();
                _died = true;
            }
            else
            {
                if (ghost.State != GhostState.Eaten)
                {
                    AddScore(500 * (LevelData.TotalGhostEaten + 1));
                    Player.AddValue(GamePlayerData.GhostEaten);
                }
            }
        }
    }
}