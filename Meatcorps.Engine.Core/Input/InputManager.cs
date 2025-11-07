using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;

namespace Meatcorps.Engine.Core.Input;

public class InputManager<T> : IBackgroundService where T : Enum
{
    private int _totalPlayers;
    private readonly PlayerInputRouter<T> _router;
    private Dictionary<int, PlayerInputStatus> _inputStatus = new Dictionary<int, PlayerInputStatus>();

    public int TotalPlayers
    {
        get => _totalPlayers;
        set { _totalPlayers = Math.Max(1, value); }
    }

    public IEnumerable<PlayerInputStatus> CurrentInputStatus
        => _inputStatus.Values;

    public bool Ready { get; private set; }

    private bool _assignMode;

    public InputManager(PlayerInputRouter<T>? router = null)
    {
        _router = router ?? GlobalObjectManager.ObjectManager.Get<PlayerInputRouter<T>>()!;
    }

    public void AssignPlayers(int totalPlayers)
    {
        if (_router.AutoAssign)
            return;
        
        _assignMode = true;
        TotalPlayers = totalPlayers;
        _inputStatus.Clear();
        for (var i = 1; i <= totalPlayers; i++)
        {
            _router.UnassignProfile(i);
            _inputStatus.Add(i, new PlayerInputStatus
            {
                Assigned = false,
                Id = i,
                Online = false,
                Type = PlayerInputType.Unknown
            });
        }
    }

    public void PreUpdate(float deltaTime)
    {
        AssignUpdateLogic();
        ValidateInputUpdateLogic();
    }

    private void ValidateInputUpdateLogic()
    {
        var ready = true;
        for (var i = 1; i <= TotalPlayers; i++)
        {
            if (!_router.IsConnected(i))
            {
                ready = false;
                break;
            }
        }

        Ready = ready;

        if (ready)
        {
            _assignMode = false;
        }
    }

    private void AssignUpdateLogic()
    {
        if (_assignMode)
        {
            if (_router.AnyInputPressed(out var profileId, out var playerId) && playerId == -1)
            {
                for (var i = 1; i <= _totalPlayers; i++)
                {
                    if (!_router.IsAssigned(i))
                    {
                        _router.AssignProfile(profileId, i);
                        _inputStatus[i] = new PlayerInputStatus
                        {
                            Assigned = true,
                            Id = i,
                            Online = _router.IsConnected(i),
                            Type = _router.InputType(i)
                        };
                        break;
                    }
                }
            }
        }

        if (_assignMode || !Ready)
            for (var i = 1; i <= _totalPlayers; i++)
            {
                if (_inputStatus[i].Online != _router.IsConnected(i))
                {
                    _inputStatus[i] = new PlayerInputStatus
                    {
                        Assigned = _router.IsAssigned(i),
                        Id = i,
                        Online = _router.IsConnected(i),
                        Type = _router.InputType(i)
                    };
                }
            }
    }

    public void Update(float deltaTime)
    {
        //
    }

    public void LateUpdate(float deltaTime)
    {
        //
    }
}

public struct PlayerInputStatus
{
    public int Id { get; init; }
    public bool Assigned { get; init; }
    public bool Online { get; init; }
    public PlayerInputType Type { get; init; }
}