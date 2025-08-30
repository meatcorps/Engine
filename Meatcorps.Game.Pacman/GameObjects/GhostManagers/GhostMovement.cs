using System.Numerics;
using Meatcorps.Engine.Collision.Utilities;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.Pathfinding.Utilities;
using Meatcorps.Game.Pacman.Data;
using Meatcorps.Game.Pacman.GameEnums;

namespace Meatcorps.Game.Pacman.GameObjects.GhostManagers;

public class GhostMovement
{
    private GhostBehaviour _behaviour;
    private PointInt _lastTargetPoint;
    private GridDistanceCalculator _distanceCalculator;
    private Vector2 _lastVelocity;
    private List<PointInt> _possibleDirections = new();
    private List<PointInt> _breadCrumb;
    private BufferedDirection _bufferedDirection;
    private Vector2 _previousDirection;
    private PointInt _lastPosition;
    private EdgeDetector _edgeDetector = new();
    private TimerOn _parkMove = new(100);
    private Vector2 _parkPosition;
    private LevelData LevelData;
    private GhostStateManager _ghostState;

    public IEnumerable<PointInt> PossibleDirections => _possibleDirections;
    
    public GhostMovement(GhostBehaviour behaviour, GridDistanceCalculator distanceCalculator, LevelData levelData, GhostStateManager stateManager, List<PointInt> breadCrumb)
    {
        _behaviour = behaviour;
        _distanceCalculator = distanceCalculator;
        LevelData = levelData;
        _ghostState = stateManager;
        _breadCrumb = breadCrumb;
        _bufferedDirection = new BufferedDirection(500);
    }
    
    public void Update(float deltaTime)
    {
        UpdateGhostScaredReverseDirection();

        UpdateDistanceMap();

        UpdateBreadCrumb();

        var home = _ghostState.State == GhostState.Eaten && _ghostState.CurrentMapItem.Position == _behaviour.Logic.GetTarget(_ghostState.State);
        if (_parkMove.NormalizedElapsed.EqualsSafe(0, 0.1f))
            _parkPosition = _behaviour.Body.Position;
        
        _parkMove.Update(home, deltaTime);
        if (home)
        {
            _behaviour.Body.Velocity = Vector2.Zero;
            _behaviour.Body.Position = Tween.Lerp(_parkPosition, LevelData.ToWorldPosition(_ghostState.CurrentMapItem.Position),
                _parkMove.NormalizedElapsed);
        }
        
        UpdateMovement(deltaTime);
        
        _behaviour.Body.Velocity = _behaviour.Body.Velocity.NormalizedCopy() * GetSpeed();
        _behaviour.Body.Position = _behaviour.Body.Position.Warp(LevelData.LevelWidth * (LevelData.GridSize),
            LevelData.LevelHeight * (LevelData.GridSize));
    }
    
    private void UpdateMovement(float deltaTime)
    {
        var speed = GetSpeed();

        if (GetBestDirection(out var raw))
        {
            _bufferedDirection.Update(raw, deltaTime);

            if (_bufferedDirection.IsDirectionChangedAndIsNotZero(_behaviour.Body.Velocity, speed, out var direction))
            {
                _behaviour.Body.SetMaxSpeed(250);
                GridMovement.TryMove(_behaviour.Body, direction, ref _lastVelocity, deltaTime,
                    LayerBits.MaskOf(CollisionLayer.Wall), 4);
            }
        }
    }

    private float GetSpeed()
    {
        if (_ghostState.State == GhostState.Eaten)
            return LevelData.Speed * 2f;
        if (LevelData.GhostScared)
            return LevelData.Speed * 0.5f;
        
        return LevelData.Speed;
    }

    private void UpdateBreadCrumb()
    {
        var currentPoint = LevelData.WorldToCell(_behaviour.Body.Position + new Vector2(LevelData.GridSize / 2f, LevelData.GridSize / 2f));
        if (currentPoint != _lastPosition)
        {
            _lastPosition = currentPoint;
            _breadCrumb.Add(currentPoint);
            if (_breadCrumb.Count > 3)
                _breadCrumb.RemoveAt(0);
        }
    }

    private void UpdateDistanceMap()
    {
        var currentPoint = _behaviour.Logic.GetTarget(_ghostState.State);
        if (currentPoint != _lastTargetPoint && LevelData.TargetPacman is not null)
        {
            _distanceCalculator.Calculate(_behaviour.Logic.GetTarget(_ghostState.State), _behaviour.Logic.GetStartDistance(), 500);
            _lastTargetPoint = currentPoint;
        }
    }

    private void UpdateGhostScaredReverseDirection()
    {
        _edgeDetector.Update(LevelData.GhostScared);

        if (_edgeDetector.IsRisingEdge)
        {
            _behaviour.Body.Velocity = -_behaviour.Body.Velocity;
            _breadCrumb.Clear();
        }
    }

    private bool GetBestDirection(out Vector2 direction)
    {
        var currentPoint = LevelData.WorldToCell(_behaviour.Body.Position + new Vector2(LevelData.GridSize / 2f, LevelData.GridSize / 2f));
        var score = int.MaxValue;
        direction = Vector2.Zero;
        _possibleDirections.Clear();

        foreach (var possibleDirection in new[]
                     { new PointInt(1, 0), new PointInt(-1, 0), new PointInt(0, 1), new PointInt(0, -1) })
        {
            var neighbour = (currentPoint + possibleDirection).Warp(LevelData.LevelWidth, LevelData.LevelHeight);
            
            if (_breadCrumb.Contains(neighbour))
                continue;
            
            var neighbourMapItem = LevelData.Map.Get(neighbour);
            var currentMapItem = LevelData.Map.Get(currentPoint);
            
            if (!neighbourMapItem.Walkable)
                continue;
            
            if (!currentMapItem.GhostHome && neighbourMapItem.OneWay && _ghostState.State != GhostState.Eaten)
                continue;

            var gridScore = _behaviour.Logic.Get(neighbour);
            
            if (LevelData.GhostScared && _ghostState.State != GhostState.Eaten && !currentMapItem.GhostHome)
                gridScore = _distanceCalculator.MaxDistance - gridScore;
            
            if (gridScore < score)
            {
                score = gridScore;
                direction = possibleDirection.ToVector2();
            }
            _possibleDirections.Add(currentPoint + possibleDirection);
        }

        return score < int.MaxValue;
    }
}