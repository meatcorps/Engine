using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Particles;
using Meatcorps.Engine.RayLib.Text;
using Meatcorps.Engine.RayLib.TweenRayLib;
using Meatcorps.Engine.RayLib.UI;
using Meatcorps.Engine.RayLib.UI.Data;

namespace Meatcorps.Engine.RayLib.GameObjects.UI;

internal sealed class UIPayload
{
    public string Text = "";
    public required TextStyle Style;
    public TweenStackColor ColorTween;
    public TweenStack SizeTween;
    public TweenStackVector2 PositionTween;
    public float TotalLifeTime;
    public Action OnStart = () => { };
}

public sealed class UIMessageEmitter : BaseGameObject
{
    private readonly ParticleSystemBuilder _particleSystem = new();
    private readonly Queue<UIPayload> _payloadPool = new();

    private readonly UIMessageStyle _defaultStyle;

    private IRenderTargetStrategy _renderTarget;

    public UIMessageEmitter(TextStyle defaultTextStyle)
    {
        Layer = 2; // ensure above gameplay
        Camera = CameraLayer.UI; // screen-space rendering

        _defaultStyle = new UIMessageStyle
        {
            Style = defaultTextStyle
        };
        _renderTarget = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()!;

        _particleSystem
            .SetMaxParticles(2)
            .SetSpawnLogic(() =>
            {
                var particle = new Particle
                {
                    LifeTime = 1f, // will be set in OnStart
                    OnStart = p =>
                    {
                        var payload = (UIPayload)p.Payload1!;
                        payload.OnStart();
                        p.LifeTime = payload.TotalLifeTime / 1000;
                    },
                    OnUpdate = p =>
                    {
                        var payload = (UIPayload)p.Payload1!;
                        p.Color = payload.ColorTween.Lerp(p.NormalizedLifetime);
                        p.Position = payload.PositionTween.Lerp(p.NormalizedLifetime);
                        p.Size = payload.SizeTween.Lerp(p.NormalizedLifetime);
                    },
                    OnDraw = p =>
                    {
                        var payload = (UIPayload)p.Payload1!;
                        var textStyle = payload.Style with { Color = p.Color, Size = p.Size };

                        TextKit.Draw(ref textStyle, payload.Text, p.Position);
                    },
                    OnEnd = p =>
                    {
                        if (_payloadPool.TryDequeue(out var payload))
                        {
                            _particleSystem.Emit(1, null, null, payload);
                        }

                        p.Payload1 = null;
                    }
                };

                return particle;
            });
    }

    private void NextMessage()
    {
        if (_payloadPool.Count != 1 || _particleSystem.TotalParticlesAlive > 0)
            return;

        if (_payloadPool.TryDequeue(out var payload))
        {
            _particleSystem.Emit(1, null, null, payload);
        }
    }

    // ---------------- Public API ----------------

    public void Show(string text, UIMessageStyle? message = null)
    {
        if (message == null)
            message = UIMessagePresets.Default(_defaultStyle.Style.Font);
        
        var totalDuration = (float)message.AppearDurationInMilliseconds + message.DisappearDurationInMilliseconds + message.HoldDurationInMilliseconds;
        var screenSize = new PointInt(_renderTarget.RenderWidth, _renderTarget.RenderHeight);
        var textSizeFrom = TextKit.Measure(message.Style with { Size = message.SizeFrom }, text);
        var textSizeTo = TextKit.Measure(message.Style with { Size = message.SizeTo }, text);
        var textSizeAfter = TextKit.Measure(message.Style with { Size = message.SizeAfter }, text);
        var positionFrom = 
            UIAnchorHelper.ResolveAnchorPixel(message.AnchorFrom, screenSize.X, screenSize.Y) - 
            UIAnchorHelper.ResolveAnchorPixel(UIAnchorHelper.InvertAnchor(message.AnchorFrom), textSizeFrom.X, textSizeFrom.Y) +
            UIAnchorHelper.AnchorToDirectionVector2(message.AnchorFrom, message.PaddingTopBottomLeftDown);
        var positionTo = 
            UIAnchorHelper.ResolveAnchorPixel(message.AnchorTo, screenSize.X, screenSize.Y) -
            UIAnchorHelper.ResolveAnchorPixel(message.AnchorTo, textSizeTo.X, textSizeTo.Y) -
            UIAnchorHelper.AnchorToDirectionVector2(message.AnchorTo, message.PaddingTopBottomLeftDown);
        var positionAfter = 
            UIAnchorHelper.ResolveAnchorPixel(message.AnchorAfter, screenSize.X, screenSize.Y) -
            UIAnchorHelper.ResolveAnchorPixel(UIAnchorHelper.InvertAnchor(message.AnchorAfter), textSizeAfter.X, textSizeAfter.Y) +
            UIAnchorHelper.AnchorToDirectionVector2(message.AnchorAfter, message.PaddingTopBottomLeftDown);

        if ((message.AnchorTo is Anchor.Bottom or Anchor.BottomLeft or Anchor.BottomRight && message.AnchorFrom is Anchor.BottomLeft or Anchor.BottomRight) || 
            (message.AnchorTo is Anchor.Top or Anchor.TopLeft or Anchor.TopRight && message.AnchorFrom is Anchor.TopLeft or Anchor.TopRight))
            positionFrom.Y = positionTo.Y;
        
        if ((message.AnchorTo is Anchor.Bottom or Anchor.BottomLeft or Anchor.BottomRight && message.AnchorAfter is Anchor.BottomLeft or Anchor.BottomRight) || 
            (message.AnchorTo is Anchor.Top or Anchor.TopLeft or Anchor.TopRight && message.AnchorAfter is Anchor.TopLeft or Anchor.TopRight))
            positionAfter.Y = positionTo.Y;
        
        var payload = new UIPayload
        {
            Text = text,
            Style = message.Style,
            ColorTween = new TweenStackColor().FromDurationInMilliseconds(
                message.ColorFrom,
                totalDuration, 
                (message.AppearDurationInMilliseconds, message.ColorTo, message.AppearEasing), 
                (message.HoldDurationInMilliseconds, message.ColorTo, EaseType.Linear), 
                (message.DisappearDurationInMilliseconds, message.ColorAfter, message.DisappearEasing)
                ),
            SizeTween = new TweenStack().FromDurationInMilliseconds(
                message.SizeFrom,
                totalDuration, 
                (message.AppearDurationInMilliseconds, message.SizeTo, message.AppearEasing), 
                (message.HoldDurationInMilliseconds, message.SizeTo, EaseType.Linear), 
                (message.DisappearDurationInMilliseconds, message.SizeAfter, message.DisappearEasing)
            ),
            PositionTween = new TweenStackVector2().FromDurationInMilliseconds(
                positionFrom,
                totalDuration, 
                (message.AppearDurationInMilliseconds, positionTo, message.AppearEasing), 
                (message.HoldDurationInMilliseconds, positionTo, EaseType.Linear), 
                (message.DisappearDurationInMilliseconds, positionAfter, message.DisappearEasing)
            ),
            TotalLifeTime = totalDuration,
            OnStart = message.AppearAction
        };
        
        _payloadPool.Enqueue(payload);
        NextMessage();
    }

    public void Countdown(int durationInMilliseconds, UIMessageStyle? style = null, UIMessageStyle? goStyle = null, string finalText = "GO!")
    {
        if (style == null)
            style = UIMessagePresets.Countdown(_defaultStyle.Style.Font);
        
        style.HoldDurationInMilliseconds = (durationInMilliseconds / 3) - style.DisappearDurationInMilliseconds - style.AppearDurationInMilliseconds;
        
        if (style.HoldDurationInMilliseconds < 0)
            throw new ArgumentException($"DisappearDurationInMilliseconds and AppearDurationInMilliseconds together must be less then {durationInMilliseconds / 3} milliseconds");
        
        for (var i = 3; i > 0; i--)
            Show(i.ToString(), style);

        if (goStyle == null)
        {
            goStyle = style;
            goStyle.HoldDurationInMilliseconds = 1000 - style.DisappearDurationInMilliseconds - style.AppearDurationInMilliseconds;
        }
        
        Show("GO!", goStyle);
    }

    public void ClearAll()
    {
        _payloadPool.Clear();
        _particleSystem.KillAll(); 
    }

    // ---------------- BaseGameObject lifecycle ----------------

    protected override void OnInitialize()
    {
    }

    protected override void OnUpdate(float deltaTime)
    {
        _particleSystem.Update(deltaTime);
    }

    protected override void OnDraw()
    {
        _particleSystem.Draw();
    }

    protected override void OnDispose()
    {
        _payloadPool.Clear();
    }
}