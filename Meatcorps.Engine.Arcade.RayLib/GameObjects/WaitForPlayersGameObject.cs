using System.Numerics;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Enums;
using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Arcade.RayLib.Utilities;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.UI;
using Meatcorps.Engine.Session;
using Raylib_cs;

namespace Meatcorps.Engine.Arcade.RayLib.GameObjects;

public class WaitForPlayersGameObject: BaseGameObject
{
    private readonly int _totalPlayersRequested;
    private readonly BaseScene _jumpToScene;
    private readonly BaseScene _timeOutToScene;
    private readonly int _fontScale;
    private IDefaultFont _font;
    private IRenderTargetStrategy _renderTarget;
    private ISessionService _sessionService;
    private IPlayerCheckin _playerCheckin;
    private Dictionary<int, bool> _playerCheckins = new();
    private Dictionary<int, bool> _playerCanPay = new();
    private bool _startingGame;
    private TimerOn _timeoutTimer = new(20000);
    private TimerOn _startTimer = new(3000);
    private InlineRender _inlineRender = new();
    private Texture2D _qrCodeTexture;
    private ArcadeServer _arcadeServer;
    private ArcadeGame _arcadeGame;
    private IArcadePointsMutator _arcadePointMutator;

    public WaitForPlayersGameObject(int totalPlayersRequested, BaseScene jumpToScene, BaseScene timeOutToScene, int fontScale = 1)
    {
#if DEBUG
        if (GlobalObjectManager.ObjectManager.Get<IUniversalConfig>()!
            .GetOrDefault("Debug", "ShortWaitForPlayerCountDown", false))
        {
            _startTimer = new TimerOn(5);
        }
#endif
        Layer = 9;
        Camera = CameraLayer.UI;
        
        _totalPlayersRequested = totalPlayersRequested;
        _jumpToScene = jumpToScene;
        _timeOutToScene = timeOutToScene;
        _fontScale = fontScale;
        for (var i = 1; i <= totalPlayersRequested; i++)
        {
            _playerCheckins.Add(i, false);
            _playerCanPay.Add(i, false);
        }
    }
    
    protected override void OnInitialize()
    {
        _arcadeServer = GlobalObjectManager.ObjectManager.Get<ArcadeServer>()!;
        _arcadeGame = GlobalObjectManager.ObjectManager.Get<ArcadeGame>()!;
        _arcadeGame.State = GameState.Waiting;
        _qrCodeTexture = QrcodeHelper.CreateTexture(_arcadeServer.AutoSignIn(_arcadeGame));
        _font = GlobalObjectManager.ObjectManager.Get<IDefaultFont>()!;
        _renderTarget = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()!;
        _sessionService = GlobalObjectManager.ObjectManager.Get<ISessionService>()!;
        _playerCheckin = GlobalObjectManager.ObjectManager.Get<IPlayerCheckin>()!;
        _arcadePointMutator = GlobalObjectManager.ObjectManager.Get<IArcadePointsMutator>()!;
        _inlineRender.Bounds = new Rect(32, 32, _renderTarget.RenderWidth - 64, _renderTarget.RenderHeight - 64);
        _inlineRender.HAlign = HAlign.Left;
        _inlineRender.ItemSpacing = 2;
        _inlineRender.LineSpacing = 2;
        _inlineRender
            .AddLabel(_font.GetFont(), null, "PAY:", 16 * _fontScale, Color.White)
            .AddLabel(_font.GetFont(), null, _arcadeGame.PricePoints.ToString(), 16 * _fontScale, Color.Red)
            .AddLabel(_font.GetFont(), null, " POINTS TO PLAY!", 16 * _fontScale, Color.White)
            .AddNewLine()
            
            .AddSpacer(4 * _fontScale, 4 * _fontScale).AddNewLine()
            
            .AddLabel(_font.GetFont(), null, "WAITING FOR PLAYERS", 16 * _fontScale, Color.White).AddNewLine()
            .AddLabel(_font.GetFont(), null, "OPEN THE WEBAPP AND USE CODE ", 10 * _fontScale, Color.White)
            .AddLabel(_font.GetFont(), null, _arcadeGame.Code.ToString(), 10 * _fontScale, Color.Green).AddNewLine()
            .AddLabel(_font.GetFont(), null, "OPEN " + _arcadeServer.Url, 10 * _fontScale, Color.White).AddNewLine()
            .AddLabel(_font.GetFont(), null, "OR SCAN THE QR CODE", 10 * _fontScale, Color.White)
            .AddNewLine()
            .AddSpacer(16 * _fontScale, 16 * _fontScale).AddNewLine();

        for (var i = 1; i <= _totalPlayersRequested; i++)
        {
            _inlineRender.AddLabel(_font.GetFont(), null, $"PLAYER {i} STATUS", 10 * _fontScale, Color.White).AddNewLine()
                .AddLabel(_font.GetFont(),"PLAY_INFO_" + i, "WAITING TO PAY " + _playerCheckin.Game.PricePoints + " POINTS", 16 * _fontScale, Color.Red).AddNewLine()
                .AddSpacer(16 * _fontScale, 16 * _fontScale).AddNewLine();
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        _timeoutTimer.Update(true, deltaTime);
        
        if (!_startingGame)
        {
            _inlineRender.Update(deltaTime);
            if (_timeoutTimer.Output)
            {
                _arcadeGame.State = GameState.Idle;
                Scene.GameHost.SwitchScene(_timeOutToScene);
                return;
            }

            var ready = true;
            foreach (var playerToCheck in _playerCheckins)
            {
                
                if (playerToCheck.Value)
                {
                    if (!_playerCheckin.IsPlayerCheckedIn(playerToCheck.Key, out var _))
                    {
                        _inlineRender.AddLabel(_font.GetFont(), "PLAY_INFO_" + playerToCheck.Key, "WAITING TO PAY " + _playerCheckin.Game.PricePoints + " POINTS", 16,
                            Color.Red);
                        _playerCheckins[playerToCheck.Key] = false;
                        _playerCanPay[playerToCheck.Key] = false;
                    }

                    if (!_playerCanPay[playerToCheck.Key])
                    {
                        if (_arcadePointMutator.GetPoints(playerToCheck.Key) >= _arcadeGame.PricePoints)
                        {
                            _playerCanPay[playerToCheck.Key] = true;
                            _playerCheckins[playerToCheck.Key] = false;
                        }
                        else
                            ready = false;
                    }

                    continue;
                }

                if (_playerCheckin.IsPlayerCheckedIn(playerToCheck.Key, out var _))
                {
                    _playerCheckins[playerToCheck.Key] = true;
                    _playerCanPay[playerToCheck.Key] =
                        _arcadePointMutator.GetPoints(playerToCheck.Key) >= _arcadeGame.PricePoints;

                    if (_playerCanPay[playerToCheck.Key])
                    {
                        _inlineRender.AddLabel(_font.GetFont(), "PLAY_INFO_" + playerToCheck.Key,  _playerCheckin.GetPlayerName(playerToCheck.Key) + " READY!", 16,
                            Color.Green);
                        continue;
                    }
            
                    _inlineRender.AddLabel(_font.GetFont(), "PLAY_INFO_" + playerToCheck.Key, "NOT ENOUGH POINTS!\nYOU NEED: " + (_arcadeGame.PricePoints - _arcadePointMutator.GetPoints(playerToCheck.Key)).ToString() + " MORE POINTS!", 10,
                        Color.Yellow);
                }

                ready = false;
                
            }

            if (ready)
            {
                _sessionService.ClearPlayers();

                foreach (var playerToCheck in _playerCheckins)
                {
                    _arcadePointMutator.RequestPoints(playerToCheck.Key, _arcadeGame.PricePoints);
                    _sessionService.TryPlayerJoin(playerToCheck.Key, _playerCheckin.GetPlayerName(playerToCheck.Key));
                }

                _startingGame = true;
            }
        }
        else
        {
            _startTimer.Update(true, deltaTime);
            if (_startTimer.Output)
            {
                _arcadeGame.State = GameState.Playing;
                Scene.GameHost.SwitchScene(_jumpToScene);
            }
        }
    }

    protected override void OnDraw()
    {
        Raylib.DrawRectangle(0, 0, _renderTarget.RenderWidth, _renderTarget.RenderHeight, Raylib.ColorAlpha(Color.Black, 0.8f));
        if (!_startingGame)
        {
            _inlineRender.Draw();
            Raylib.DrawTexturePro(_qrCodeTexture, new Rectangle(0, 0, _qrCodeTexture.Width, _qrCodeTexture.Height), new Rectangle(_renderTarget.RenderWidth - 150, 16, 150, 150), Vector2.Zero, 0, Color.White);
            var countText = (_timeoutTimer.TimeRemaining / 1000).ToString("F0");
            var size = Raylib.MeasureTextEx(_font.GetFont(), "TIMEOUT IN " + countText + "S", 16f * _fontScale, 1);
            Raylib.DrawTextEx(_font.GetFont(), "TIMEOUT IN " + countText + "S", new Vector2(_renderTarget.RenderWidth - 16 - size.X, _renderTarget.RenderHeight - 16 - size.Y), 16f  * _fontScale, 1, Color.Red);
        }
        else
        {
            var size = Raylib.MeasureTextEx(_font.GetFont(), "STARTING GAME IN", 32f * _fontScale, 1);
            Raylib.DrawTextEx(_font.GetFont(), "STARTING GAME IN", new Vector2((_renderTarget.RenderWidth - size.X) / 2, 64), 32f * _fontScale, 1, Color.Magenta);
            Raylib.DrawTextEx(_font.GetFont(), (_startTimer.TimeRemaining / 1000).ToString("F0"), new Vector2(((float)_renderTarget.RenderWidth / 2) - (64f * _fontScale / 2), ((float)_renderTarget.RenderHeight / 2) - (64f * _fontScale / 2)), 64f * _fontScale, 1, Color.White);
        }
    }

    protected override void OnDispose()
    {
        Raylib.UnloadTexture(_qrCodeTexture);
    }
}