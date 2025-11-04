using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.Pathfinding.Utilities;
using Meatcorps.Game.CyberMaze.Data;
using Meatcorps.Game.CyberMaze.GameEnums;
using Meatcorps.Game.CyberMaze.GameObjects.Abstractions;
using Raylib_cs;

namespace Meatcorps.Game.CyberMaze.GameObjects;

public class TargetSeekerGameObject: ResourceGameObject
{
    private GridDistanceCalculator _gridDistanceCalculator;
    private PointInt _previousPosition;
    private Dictionary<int, List<PointInt>> _ghostToCyberPlayerPath = new();
    private Dictionary<int, PointInt> _previousGhostPositions = new();
    private CyberPlayerTargetFinder _cyberPlayerTargetFinder;
    private TimerOn _animationTimer = new(1000);
    private FixedTimer _animationBlinkTimer = new(50);
    private TimerOn _showTargetTimer = new(3000);
    private bool _renderCircle;

    protected override void OnInitialize()
    {
        Enabled = false;
        base.OnInitialize();
        LevelData.TargetSeeker = this;
        Layer = 4;
        _cyberPlayerTargetFinder = new CyberPlayerTargetFinder(LevelData);
        _gridDistanceCalculator = new GridDistanceCalculator(_cyberPlayerTargetFinder).Set4AllowedDirections();
        for (var i = 0; i < 4; i++)
        {
            _ghostToCyberPlayerPath.Add(i, new List<PointInt>());
            _previousGhostPositions.Add(i, new PointInt(0, 0));
        }
    }

    protected override void OnEnabled()
    {
        _animationTimer.Reset();
        base.OnEnabled();
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (LevelData.TargetCyberPlayer == null)
            return;
        
        if (LevelData.GhostScared)
            Enabled = false;
       
        _animationTimer.Update(true, deltaTime);
        _showTargetTimer.Update(_animationTimer.Output, deltaTime);
        
        if (_showTargetTimer.Output)
            Enabled = false;
        
        _animationBlinkTimer.Update(deltaTime);
        _showTargetTimer.Update(_renderCircle, deltaTime);

        var changed = false;
        
        for (var i = 0; i < 4; i++)
        {
            var currentPosition = LevelData.WorldToCell(LevelData.Ghosts[i].Body.BoundingBox.Center);
            if (_previousGhostPositions[i] != currentPosition)
            {
                changed = true;
                _previousGhostPositions[i] = currentPosition;
            }
        }
        
        var currentCyberPlayerPosition = LevelData.WorldToCell(LevelData.TargetCyberPlayer.Body.BoundingBox.Center);
        if (_previousPosition != currentCyberPlayerPosition || changed)
        {
            _gridDistanceCalculator.Calculate(currentCyberPlayerPosition);
            for (var i = 0; i < 4; i++)
                _cyberPlayerTargetFinder.GetPath(_previousGhostPositions[i],
                    currentCyberPlayerPosition, _ghostToCyberPlayerPath[i]);
            
            _previousPosition = currentCyberPlayerPosition;
        }

        if (_animationTimer.Output && _animationBlinkTimer.Output && !DemoMode)
            Sounds.Play(GameSounds.Shortblip, 1, 0.25f + _showTargetTimer.NormalizedElapsed * 0.25f);
    }

    protected override void OnDraw()
    {
        if (LevelData.TargetCyberPlayer == null)
            return;
        
        _renderCircle = false;
        var cyberPlayerPosition = LevelData.TargetCyberPlayer.Body.BoundingBox.Center;
        var maxDistance = new Vector2(32, 0).LengthSquared();
        foreach (var path in _ghostToCyberPlayerPath.Values)
        {
            if (path.Count() < 4)
                continue;
            var previous = LevelData.ToWorldRectangle(path[0]).Center.ToVector2();
            var totalPathsToRender = (int)((float)(path.Count - 2) * _animationTimer.NormalizedElapsed);
            for (var i = 1; i <= totalPathsToRender; i++)
            {
                var current = LevelData.ToWorldRectangle(path[i]).Center.ToVector2();
                if ((previous - current).LengthSquared() < maxDistance)
                {
                    var color = Raylib.ColorAlpha(Color.Red, Math.Clamp((float)i / totalPathsToRender, 0, 1));
                    Raylib.DrawLineEx(previous, current, 2, color);
                }

                previous = current;
            }

            var currentCyberPlayer = cyberPlayerPosition + (previous - cyberPlayerPosition).NormalizedCopy() * 16;
            if (path.Count - totalPathsToRender < 4 && (previous - currentCyberPlayer).LengthSquared() < maxDistance)
            {
                _renderCircle = true;
                Raylib.DrawLineEx(previous, currentCyberPlayer, 2,
                    Color.Red);
            }
        }

        if (_renderCircle)
        {
            if (_animationBlinkTimer.NormalizedElapsed > 0.5f)
            {
                Raylib.DrawCircleLinesV(cyberPlayerPosition, 16, Color.Red);
                Raylib.DrawCircleLinesV(cyberPlayerPosition, 16.5f, Color.Red);
                Raylib.DrawCircleLinesV(cyberPlayerPosition, 17f, Color.Red);
                Raylib.DrawTextPro(Fonts.GetFont(), "TARGET\n>FOUND", cyberPlayerPosition + new Vector2(-26, 18),
                    Vector2.Zero, 0, 8, 1, Color.White);
            }
        }
    }
    
    protected override void OnDispose()
    {
    }
}