using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;

namespace Meatcorps.Engine.Core.Input;

/// <summary>
/// Manages the player assignment state machine for an InputMapper. In assign mode, it listens for any pressed input and assigns that profile to the next unoccupied player slot. Ready becomes true once all expected player slots are connected.
/// </summary>
public class InputManager<T> : IBackgroundService where T : Enum
{
    private int _totalPlayers;
    private readonly PlayerInputRouter<T> _router;
    private readonly Dictionary<int, PlayerInputStatus> _inputStatus = new Dictionary<int, PlayerInputStatus>();

    /// <summary>Number of player slots expected. Minimum 1.</summary>
    public int TotalPlayers
    {
        get => _totalPlayers;
        set { _totalPlayers = Math.Max(1, value); }
    }

    /// <summary>Current assignment and connection status for each player slot.</summary>
    public IEnumerable<PlayerInputStatus> CurrentInputStatus
        => _inputStatus.Values;

    /// <summary>true when all expected player slots have an assigned, connected profile.</summary>
    public bool Ready { get; private set; }

    private bool _assignMode;

    public InputManager(PlayerInputRouter<T>? router = null)
    {
        _router = router ?? GlobalObjectManager.ObjectManager.Get<PlayerInputRouter<T>>()!;
    }

    /// <summary>Enters assign mode for the given number of player slots. Clears existing assignments and waits for players to press any input to claim a slot. No-op if the router uses AutoAssign.</summary>
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

/// <summary>Describes the assignment and connection state of a single player slot.</summary>
public struct PlayerInputStatus
{
    /// <summary>The player slot index.</summary>
    public int Id { get; init; }

    /// <summary>true if a profile has been assigned to this slot.</summary>
    public bool Assigned { get; init; }

    /// <summary>true if the assigned profile is currently connected.</summary>
    public bool Online { get; init; }

    /// <summary>The input device type for this slot.</summary>
    public PlayerInputType Type { get; init; }
}