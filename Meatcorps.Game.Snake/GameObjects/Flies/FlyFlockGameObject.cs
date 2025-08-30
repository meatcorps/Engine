using System.Numerics;
using Meatcorps.Engine.Boids.Data;
using Meatcorps.Engine.Boids.Interfaces;
using Meatcorps.Engine.Boids.Services;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Interfaces.Grid;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Game.Snake.Data;
using Meatcorps.Game.Snake.GameObjects.Abstractions;
using Meatcorps.Game.Snake.Resources;
using Raylib_cs;

namespace Meatcorps.Game.Snake.GameObjects.Flies;

public class FlyFlockGameObject : SnakeGameObject 
{
    private readonly BoidConfig _config;
    private readonly FlockController _flock;
    private readonly List<FlyAgent> _agents = new();
    private const float _targetCatchRadius = 12f;
    private OneSoundManager _buzzSound;
    
    public FlyFlockGameObject(
        BoidConfig config,
        ISpatialEntityGrid grid,
        RectF bounds,
        ITargetSelector targetSelector)
    {
        _config = config;
        Layer = 5;
        _flock = new FlockController(config, grid);
        _flock.SetBounds(bounds, config.BoundsPolicy);
        _flock.SetTargetSelector(targetSelector);
        _flock.OnNearTarget += HandleNearTarget; 
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        
        _buzzSound = Sounds.GetOneSoundManager(SnakeSounds.Insectswarm);
        _buzzSound.Repeat = false;
    }

    public void AddFly(FlyAgent agent)
    {
        _agents.Add(agent);
        _flock.AddAgent(agent, agent);
    }
    
    public void SpawnRandom(int count, RectF area, object parentTag = null)
    {
        for (var i = 0; i < count; i++)
        {
            var x = area.X + Random.Shared.NextSingle() * area.Width;
            var y = area.Y + Random.Shared.NextSingle() * area.Height;

            var fly = new FlyAgent(parentTag ?? this, new Vector2(x, y));
            AddFly(fly); 
        }
    }

    public void KillAll()
    {
        _agents.Clear();
        _flock.Clear();
    }

    protected override void OnPreUpdate(float deltaTime)
    {
        // Optional pre-movement effects here
    }

    protected override void OnUpdate(float deltaTime)
    {
        var totalAgents = _agents.Count;
        _buzzSound.Repeat = totalAgents > 0;
        var nowSeconds = (float)Raylib_cs.Raylib.GetTime();
        _flock.Update(deltaTime, nowSeconds);
        
        if (totalAgents == 0)
            return;
        
        var averageVelocity = new Vector2();
        foreach (var agent in _agents)
        {
            agent.Update(deltaTime);
            averageVelocity += agent.Velocity;
        }

        
        averageVelocity /= _agents.Count;
        var averageVelocityLength = averageVelocity.Length() / _config.MaxSpeed;
        _buzzSound.Volume = (averageVelocityLength * 0.8f + 0.2f) * 0.8f;
        _buzzSound.Pitch = Tween.Lerp(0.8f, 1.2f, averageVelocityLength);
    }

    protected override void OnDisabled()
    {
        _buzzSound.Volume = 0;
        base.OnDisabled();
    }

    protected override void OnLateUpdate(float deltaTime)
    {
        // Optional cleanup or event processing
    }

    private void HandleNearTarget(IBoidAgent agent, Vector2 targetWorldPos)
    {
        // Map world → grid cell
        var cell = LevelData.WorldToCell(targetWorldPos);

        // Try to get a consumable at this cell
        if (LevelData.ConsumableGrid.TryGet(cell, out var con) && con.IsMeat)
        {
            con.AddDecay();
            _flock.SetFrenzy(1 - con.RottenNormalized);
        }
    }
    
    protected override void OnDraw()
    {
        foreach (var agent in _agents)
        {
            // Minimal triangle or sprite draw
            var forward = agent.Velocity.NormalizedSafe(Vector2.UnitX);
            var angleRad = forward.GetAngleWithOrientation(Vector2.UnitX);
            var angleDeg = MathHelper.ToDegrees(angleRad);
            Sprites.DrawAnimation(SnakeSprites.FlyAnimation, agent.CurrentAnimationFrame, agent.Position, Color.White, angleDeg, new Vector2(8, 8));
        }
    }

    protected override void OnDispose()
    {
        _flock.OnNearTarget -= HandleNearTarget; 
        _buzzSound.Dispose();
    }
}