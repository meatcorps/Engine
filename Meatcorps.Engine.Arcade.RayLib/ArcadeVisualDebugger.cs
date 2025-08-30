using System.Numerics;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Game;
using Meatcorps.Engine.RayLib.Renderer;
using Meatcorps.Engine.RayLib.Scenes;
using Raylib_cs;

namespace Meatcorps.Engine.Arcade.RayLib;

public class ArcadeVisualDebugger : BaseGameObject, IBackgroundService
{
    private RenderService? _renderService = null;
    private IArcadePointsMutator _pointsMutator;
    private IPlayerCheckin _playerCheckin;
    private FallbackArcadeSystem? _fallbackMutator;
    private bool _isInitilized;
    private Dictionary<KeyboardKey, GenericInput> _inputMap = new();
    private ArcadeGame _game;

    public ArcadeVisualDebugger()
    {
        MapKey(KeyboardKey.F1);
        MapKey(KeyboardKey.F2);
        MapKey(KeyboardKey.F3);
        MapKey(KeyboardKey.F4);
        MapKey(KeyboardKey.F5);
        MapKey(KeyboardKey.F6);
        MapKey(KeyboardKey.F7);
        MapKey(KeyboardKey.F8);
        MapKey(KeyboardKey.F11);
        Camera = CameraLayer.UI;
        Layer = 11;
    }

    private void MapKey(KeyboardKey key)
    {
        _inputMap.Add(key, new GenericInput(() => Raylib_cs.Raylib.IsKeyDown(key), key.ToString()));
    }
    
    protected override void OnInitialize()
    {
        if (_isInitilized)
            return;
        SetScene(new EmptyScene());
        _game = GlobalObjectManager.ObjectManager.Get<ArcadeGame>()!;
        _renderService = GlobalObjectManager.ObjectManager.Get<GameHost>()!.RenderService;
        _pointsMutator = GlobalObjectManager.ObjectManager.Get<IArcadePointsMutator>()!;
        _playerCheckin = GlobalObjectManager.ObjectManager.Get<IPlayerCheckin>()!;
        if (_playerCheckin is FallbackArcadeSystem fallback)
            _fallbackMutator = fallback;
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (_renderService == null)
            OnInitialize();
        
        FallBackLogic();
        
        if (Visible && Enabled)
            _renderService!.RegisterRender(this);
    }

    private void FallBackLogic()
    {
        if (_fallbackMutator is null)
            return; 
        
        foreach (var input in _inputMap.Values)
            input.Update();
        
        if (_inputMap[KeyboardKey.F1].IsPressed)
            _fallbackMutator.SignPlayerIn();
        if (_inputMap[KeyboardKey.F2].IsPressed)
            _fallbackMutator.SignPlayerOut(1);
        if (_inputMap[KeyboardKey.F3].IsPressed)
            _fallbackMutator.SignPlayerOut(2);
        if (_inputMap[KeyboardKey.F5].IsPressed)
            _fallbackMutator.SubmitPoints(1, 100);
        if (_inputMap[KeyboardKey.F6].IsPressed)
            _fallbackMutator.SubmitPoints(2, 100);
        if (_inputMap[KeyboardKey.F7].IsPressed)
            _fallbackMutator.RequestPoints(1, 100);
        if (_inputMap[KeyboardKey.F8].IsPressed)
            _fallbackMutator.RequestPoints(2, 100);
        if (_inputMap[KeyboardKey.F4].IsPressed)
            Visible = !Visible;
        if (_inputMap[KeyboardKey.F11].IsPressed)
            GlobalObjectManager.ObjectManager.Get<GameHost>()!.ToggleFullscreen();
    }

    protected override void OnDraw()
    {
        var currentY = 16;
        Raylib_cs.Raylib.DrawTextEx(Raylib_cs.Raylib.GetFontDefault(),$"S: ({_game.State}): [F1] ADD PLAYER, [F2,F3] SIGNOUT [F5,F6] ADD 100 [F7,F8] SUB 100 [F4] SHOW/HIDE", new Vector2(16, currentY), 10f, 1, Color.White);
        currentY += 11;
        for (var i = 1; i <= _playerCheckin.TotalPlayers; i++)
        {
            var str = $"{i}: '{_playerCheckin.GetPlayerName(i)}' P: {_pointsMutator.GetPoints(i)}";
               Raylib_cs.Raylib.DrawTextEx(Raylib_cs.Raylib.GetFontDefault(), str, new Vector2(16, currentY), 10f, 1, Color.White);
               currentY += 11;
        }
        
        if (_playerCheckin.TotalPlayers == 0)
            Raylib_cs.Raylib.DrawTextEx(Raylib_cs.Raylib.GetFontDefault(), "NOBODY CHECKED IN YET", new Vector2(16, currentY), 10f, 1, Color.White);
            
        
        base.OnDraw();
    }

    protected override void OnDispose()
    {
    }
}