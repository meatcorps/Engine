using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Game.KillTheSkulls.Data;
using Meatcorps.Game.KillTheSkulls.GameEnums;
using Meatcorps.Game.KillTheSkulls.GameObjects.Abstractions;
using Meatcorps.Game.KillTheSkulls.Scenes;
using Raylib_cs;

namespace Meatcorps.Game.KillTheSkulls.GameObjects;

public class RowManager : ResourceGameObject
{
    private readonly LevelRow _levelRow;
    private readonly int _rowNumber;
    private readonly PlayerInputRouter<GameInput> _controller;
    private readonly GameInput _targetInput;
    private LevelScene _levelScene;
    private CameraControllerGameObject _camera;
    private EnemyState _previousEnemyState;

    public RowManager(LevelRow levelRow, int rowNumber)
    {
        _levelRow = levelRow;
        _rowNumber = rowNumber;
        _controller = GlobalObjectManager.ObjectManager.Get<PlayerInputRouter<GameInput>>()!;
        switch (rowNumber)
        {
            case 0:
                _targetInput = GameInput.Smash1White;
                break;
            case 1:
                _targetInput = GameInput.Smash2Blue;
                break;
            case 2:
                _targetInput = GameInput.Smash3Yellow;
                break;
            case 3:
                _targetInput = GameInput.Smash4Green;
                break;
            case 4:
                _targetInput = GameInput.Smash5Red;
                break;
        }
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        _levelScene = (Scene as LevelScene)!;
        _camera = Scene.GetGameObject<CameraControllerGameObject>()!;
    }

    protected override void OnUpdate(float deltaTime)
    {
        var input = _controller.GetState(1, _targetInput);
        var pressed = input.IsPressed;
       
        if (DemoMode)
        {
            if (Raylib.GetRandomValue(0, 100) < 10)
                pressed = true;
        }

        if (pressed)
        {
            if (!DemoMode)
                _camera.Shake(4);
            
            _levelRow.Thunder.Start();
            if (_levelRow.Enemy.InRange && _levelRow.Thunder.IsRunning)
                _levelRow.Enemy.Die();
            else
                (Scene as LevelScene)!.EnemyAttack();
        }

        if (_levelScene.RandomNumber == _rowNumber)
            _levelRow.Enemy.Start();

        if (_levelRow.Enemy.Died)
        {
            _levelRow.Hit = true;
        }

        if (_levelRow.Enemy.Attacked)
            _levelRow.Miss = true;

        _levelRow.LedBar.Mode = LedBarMode.On;

        if (_levelRow.Enemy.State == EnemyState.Attacking)
            _levelRow.LedBar.Mode = LedBarMode.Error;

        if (_levelRow.Enemy.State == EnemyState.Waiting)
        {
            _levelRow.LedBar.Mode = LedBarMode.Blinking;
            if (!DemoMode && _previousEnemyState != _levelRow.Enemy.State)
                input.Animation = new BlinkAnimation(100);
        }
        else if (_levelRow.Enemy.State == EnemyState.Up)
        {
            _levelRow.LedBar.Mode = LedBarMode.Wave;
            if (!DemoMode && _previousEnemyState != _levelRow.Enemy.State)
                input.Animation = new BlinkAnimation(500);
        }
        else if (_levelRow.Enemy.State == EnemyState.Dying)
        {
            _levelRow.LedBar.Mode = LedBarMode.Charge;
            _levelRow.LedBar.Charge = _levelRow.Enemy.DieNormal;
            if (!DemoMode && _previousEnemyState != _levelRow.Enemy.State)
                input.Animation = new BlinkAnimation(50);
        }
        else
        {
            if (!DemoMode)
                input.Animation = null;
        }
        
        if (_levelRow.Enemy.State is EnemyState.Waiting or EnemyState.Up && !DemoMode)
            input.EnableLightWhenPressed = true;
        
        _levelRow.Enemy.Died = false;
        _levelRow.Enemy.Attacked = false;
        _previousEnemyState = _levelRow.Enemy.State;
    }

    protected override void OnDispose()
    {
    }
}