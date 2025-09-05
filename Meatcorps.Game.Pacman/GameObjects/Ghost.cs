using System.Numerics;
using Meatcorps.Engine.Collision.Data;
using Meatcorps.Engine.Collision.Enums;
using Meatcorps.Engine.Collision.Interfaces;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.Pathfinding.Utilities;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Pathfinding.Extensions;
using Meatcorps.Game.Pacman.Data;
using Meatcorps.Game.Pacman.GameEnums;
using Meatcorps.Game.Pacman.GameObjects.Abstractions;
using Meatcorps.Game.Pacman.GameObjects.GhostManagers;
using Raylib_cs;

namespace Meatcorps.Game.Pacman.GameObjects;

public class Ghost : ResourceGameObject, ICollisionEvents
{
    private GhostBehaviour _behaviour;
    private GridDistanceCalculator _distanceCalculator;
    private List<PointInt> _breadCrumb = new(3);
    private BufferedDirection _bufferedDirection;
    private GhostStateManager _ghostState;
    private GhostMovement _ghostMovement;
    private FixedTimer _animationTimer = new(200);
    private FixedTimer _warningTimer = new(200);

    private RandomEnum<GameSounds> _ghostEatenSounds = new RandomEnum<GameSounds>()
        .Add(GameSounds.Nlpycho2, 25)
        .Add(GameSounds.Nlpyscho, 25)
        .Add(GameSounds.Nlpyscho4, 25);
    
    public GhostState State => _ghostState.State;
    
    public void SetBehaviour(GhostBehaviour behaviour)
    {
        _behaviour = behaviour;
        _distanceCalculator = new GridDistanceCalculator(behaviour.Logic)
            .Set4AllowedDirections();
    }

    protected override void OnInitialize()
    {
        Layer = 4;
        Enabled = false;
        base.OnInitialize();
        _ghostState = new GhostStateManager(LevelData, _behaviour, _breadCrumb);
        _ghostState.SetTimers();
        _ghostMovement = new GhostMovement(_behaviour, _distanceCalculator, LevelData, _ghostState, _breadCrumb);
    }

    protected override void OnPreUpdate(float deltaTime)
    {
        base.OnPreUpdate(deltaTime);

        _ghostState.PreUpdate();
    }

    protected override void OnUpdate(float deltaTime)
    {
        _warningTimer.Update(deltaTime);
        _animationTimer.Update(deltaTime);
        _ghostState.Update(deltaTime);
        
        if (LevelData.FreezePlayersAndGhosts)
        {
            _behaviour.Body.Velocity = Vector2.Zero;
            return;
        }

        if (_ghostState.State == GhostState.Idle)
            return;

        if (LevelData.TargetPacman is null)
            return;

        _ghostMovement.Update(deltaTime);
    }

    protected override void OnAlwaysUpdate(float deltaTime)
    {
        Enabled = _behaviour.Enabled;
        base.OnAlwaysUpdate(deltaTime);
    }

    protected override void OnLateUpdate(float deltaTime)
    {
        _ghostState.PostUpdate(deltaTime);
        base.OnLateUpdate(deltaTime);
    }

    public void OnContact(ContactPhase phase, in ContactPair pair, in ContactManifold manifold)
    {
    }

    public void SetTimers()
    {
        _ghostState.SetTimers();
        _ghostState.ResetTimers();
    }
    
    public void OnTrigger(ContactPhase phase, in ContactPair pair)
    {
        if (pair.TryGetOwner<PacMan>(out var _) && pair.TryGetOwner<PowerPellet>(out var _))
        {
            _ghostState.ResetTimers();
            return;
        }
            
        
        if (!pair.ContainsOwner(this))
            return;
        
        if (pair.TryGetOwner<PacMan>(out var pacMan))
        {
            if (LevelData.GhostScared)
            {
                _ghostState.IsEaten();
                GhostScared();
                CameraManager.Shake(1.5f);
            }
        }
    }

    private void GhostScared()
    {
        if (DemoMode)
            return;
        
        if (LevelData.DutchMode)
        {
            Sounds.Play(_ghostEatenSounds.Get());
        }
        else
        {
            Sounds.Play(GameSounds.Bang);
        }
    }

    protected override void OnDraw()
    {
        var position = _behaviour.Body.Position;
        var facing = _behaviour.Body.Velocity.NormalizedCopy().ToPointInt();
        
        if (_ghostState.State != GhostState.Eaten)
        {
            var animation = Math.Clamp(Tween.ApplyEasing(Tween.NormalToUpDown(_animationTimer.NormalizedElapsed), EaseType.EaseInOut), 0, 1);
            if (LevelData.GhostScared)
            {
                Sprites.DrawAnimationWithNormal(GameSprites.GhostScaredAnimation,
                    animation,
                    position, Raylib.ColorAlpha(Color.White, 0.8f));
            }
            else
            {
                switch (_behaviour.Type)
                {
                    case GhostType.Blinky:
                        Sprites.DrawAnimationWithNormal(GameSprites.GhostBlinkyAnimation,
                            animation,
                            position, Raylib.ColorAlpha(Color.White, 0.8f));
                        break;
                    case GhostType.Pinky:
                        Sprites.DrawAnimationWithNormal(GameSprites.GhostPinkyAnimation,
                            animation ,
                            position, Raylib.ColorAlpha(Color.White, 1f));
                        break;
                    case GhostType.Inky:
                        Sprites.DrawAnimationWithNormal(GameSprites.GhostInkyAnimation,
                            animation,
                            position, Raylib.ColorAlpha(Color.White, 1f));
                        break;
                    case GhostType.Clyde:
                        Sprites.DrawAnimationWithNormal(GameSprites.GhostClydeAnimation,
                            animation,
                            position, Raylib.ColorAlpha(Color.White, 1f));
                        break;
                }
            }
        }
        
        if (_ghostState.State == GhostState.Eaten)
        {
            Sprites.Draw(GameSprites.GhostShutdown, position + facing.ToVector2());
        } else if (LevelData.GhostScared)
        {
            Sprites.Draw(GameSprites.GhostScared, position + facing.ToVector2());
        }
        else
        {
            Sprites.Draw(GameSprites.GhostAngry, position + facing.ToVector2());
        }
        
        if (((_ghostState.State == GhostState.Eaten && !_ghostState.TimeLeft.EqualsSafe(0)) || (LevelData.GhostScared && LevelData.TotalGhostEaten == 0)) && ((_warningTimer.NormalizedElapsed > 0.5f && _ghostState.TimeLeft < 2000) || _ghostState.TimeLeft >= 2000))
            Raylib.DrawTextEx(Fonts.GetFont(), (_ghostState.TimeLeft / 1000).ToString("F1"), position + new Vector2(-1, -7), 6, 0, Color.White);
        //Raylib.DrawTextEx(Fonts.GetFont(), _behaviour.Body.Velocity.ToString(), position + new Vector2(-1, -7), 6, 0, Color.White);
        //if (_behaviour.Type == GhostType.Blinky)
        //    _distanceCalculator.DrawHeatMap(LevelData.ToWorldRectangle(PointInt.Zero), 0.2f, true);
    }

    protected override void OnDispose()
    {
    }
}