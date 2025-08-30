using Meatcorps.Engine.Collision.Utilities;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Game.Pacman.Data;
using Meatcorps.Game.Pacman.GameEnums;

namespace Meatcorps.Game.Pacman.GameObjects.GhostManagers;

public class GhostStateManager
{
    private LevelData LevelData { get; }
    private GhostBehaviour _behaviour;
    private readonly List<PointInt> _breadCrumb;
    private GhostState _state;
    private GhostState _previousState;
    private TimerOn _chaseTimer;
    private TimerOn _scatterTimer;
    private TimerOn _startTimer;
    private TimerOn _scaredTimer = new(10000);
    private TimerOn _stayHomeTimer = new(3000);

    public void ResetTimers()
    {
        _chaseTimer.Reset();
        _scatterTimer.Reset();
        _startTimer.Reset();
        _scaredTimer.Reset();
        _stayHomeTimer.Reset();
    }
    
    public float TimeLeft
    {
        get
        {
            if (_state == GhostState.Eaten)
                return _stayHomeTimer.TimeRemaining;
            if (LevelData.GhostScared)
                return _scaredTimer.TimeRemaining;
            if (_state == GhostState.Chase)
                return _chaseTimer.TimeRemaining;
            if (_state == GhostState.Scatter)
                return _scatterTimer.TimeRemaining;
            return 0;
        }
    }
    
    public GhostState State => _state;
    public MapItem CurrentMapItem { get; private set; }

    public GhostStateManager(LevelData levelData, GhostBehaviour behaviour, List<PointInt> breadCrumb)
    {
        _behaviour = behaviour;
        _breadCrumb = breadCrumb;
        LevelData = levelData;
    }

    public void SetTimers()
    {
        _scatterTimer = new TimerOn(LevelData.ScatterTime);
        _chaseTimer = new TimerOn(LevelData.ChaseTime);
        _startTimer = new TimerOn(_behaviour.Logic.TimeoutBeforeStart);
        _scaredTimer = new TimerOn(LevelData.ScaredTime);
        _stayHomeTimer = new TimerOn(LevelData.StayAtHomeTime);
    }

    public void PreUpdate()
    {
        if (_state == GhostState.Eaten && !_stayHomeTimer.Output)
            LevelData.TotalGhostEaten++;
    }
    
    public void Update(float deltaTime)
    {
        _startTimer.Update(true, deltaTime);
        if (_startTimer.Output && _state == GhostState.Idle) 
            _state = GhostState.Chase;
        
        if (_state != _previousState)
        {
            _previousState = _state;
            if (_state == GhostState.Chase)
                LevelData.TargetSeeker!.Enabled = true;
        }
        
        if (_state == GhostState.Idle)
            return;
        
        UpdateChaseAndScatterTimers(deltaTime);
        
        if (_stayHomeTimer.Output && LevelData.TotalGhostEaten == 0)
        {
            _state = GhostState.Chase;
            LevelData.GhostScared = false;
            _breadCrumb.Clear();
            LevelData.HomeTaken.Clear();
        }
        
        CurrentMapItem = LevelData.Map.Get(LevelData.WorldToCell(_behaviour.Body.Position));
        
        _stayHomeTimer.Update(_state == GhostState.Eaten && CurrentMapItem.GhostHome, deltaTime);
    }

    public void IsEaten()
    {
        if (LevelData.GhostScared && _state != GhostState.Eaten)
        {
            _state = GhostState.Eaten;
            LevelData.HomeTaken.Add(_behaviour.Logic, _behaviour.Logic.GetTarget(_state));
        }
    }

    public void PostUpdate(float deltaTime)
    {
        _scaredTimer.Update(LevelData.GhostScared && !LevelData.GhostScaredResetTimer && LevelData.TotalGhostEaten == 0, deltaTime);
        if (_scaredTimer.Output)
            LevelData.GhostScared = false;

    }

    private void UpdateChaseAndScatterTimers(float deltaTime)
    {
        _chaseTimer.Update(_state == GhostState.Chase, deltaTime);
        _scatterTimer.Update(_state == GhostState.Scatter, deltaTime);
        
        if (_chaseTimer.Output && _state == GhostState.Chase)
        {
            _state = GhostState.Scatter;
        }

        if (_scatterTimer.Output && _state == GhostState.Scatter)
        {
            _state = GhostState.Chase;
        }
    }

}