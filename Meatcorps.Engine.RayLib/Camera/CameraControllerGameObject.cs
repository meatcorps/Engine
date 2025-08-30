using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.RayLib.Camera;

public class CameraControllerGameObject : BaseGameObject
{
    private readonly ICamera _camera;

    // --- Smooth Follow ---
    private BaseGameObject? _target = null;
    private Vector2 _followOffset = Vector2.Zero;
    private float _followSpeed = 8f;

    // --- Function-based Follow (alternative)
    private Func<Vector2>? _targetGetter = null;

    // --- Smooth Zoom ---
    private float _targetZoom = 0f;
    private float _zoomSpeed = 6f;

    // --- Shake ---
    private float _shakeIntensity = 0f;
    private float _shakeDecay = 6f;
    private Vector2 _shakeOffset = Vector2.Zero;

    // --- Defaults & Bounds ---
    private Vector2 _defaultPosition;
    private float _defaultZoom;
    private Rect? _bounds = null;

    public CameraControllerGameObject(ICamera? camera)
    {
        Enabled = camera is not null;
        Visible = false;
        
        if (camera is null)
            return;
        
        _camera = camera;

        _defaultPosition = camera.Position;
        _defaultZoom = camera.Zoom;
    }

    public void SetPosition(Vector2 position)
    {
        Position = position;
    }
    
    // ------------------- Public API -------------------

    public CameraControllerGameObject Follow(BaseGameObject target, Vector2? offset = null, float followSpeed = 8f)
    {
        _target = target;
        _followOffset = offset ?? Vector2.Zero;
        _followSpeed = followSpeed;
        _targetGetter = null; // disable delegate follow if active
        return this;
    }

    public CameraControllerGameObject Follow(Func<Vector2> targetGetter, float followSpeed = 8f)
    {
        _targetGetter = targetGetter;
        _followSpeed = followSpeed;
        _target = null; // disable gameobject follow if active
        return this;
    }

    public CameraControllerGameObject SetZoom(float zoom, float speed = 6f)
    {
        _targetZoom = zoom;
        _zoomSpeed = speed;
        return this;
    }

    public CameraControllerGameObject Shake(float intensity, float decay = 6f)
    {
        _shakeIntensity = intensity;
        _shakeDecay = decay;
        return this;
    }

    public CameraControllerGameObject SetBounds(Rect bounds)
    {
        _bounds = bounds;
        return this;
    }

    public void Reset()
    {
        _camera.Position = _defaultPosition;
        _camera.Zoom = _defaultZoom;
        _targetZoom = _defaultZoom;
        _shakeOffset = Vector2.Zero;
        _shakeIntensity = 0f;
        _target = null;
        _targetGetter = null;
    }

    // ------------------- Internal Logic -------------------

    protected override void OnInitialize()
    {
        if (_camera is null)
            return;
        _camera.Position = _defaultPosition;
        _camera.Zoom = _defaultZoom;
    }

    protected override void OnUpdate(float deltaTime)
    {
        Vector2? followTarget = null;

        if (_target != null)
            followTarget = _target.Position + _followOffset;
        else if (_targetGetter != null)
            followTarget = _targetGetter();

        if (followTarget.HasValue)
        {
            var desired = followTarget.Value;

            // Smooth follow using easing
            var newPos = Tween.Lerp(
                _camera.Position,
                desired,
                Tween.ApplyEasing(_followSpeed * deltaTime, EaseType.EaseInOut)
            );

            // Clamp to bounds if applicable
            if (_bounds.HasValue && _camera is ICameraFixedWidthAndHeight cameraFixedWidthAndHeight)
            {
                var viewSize = new Vector2(
                    cameraFixedWidthAndHeight.TargetWidth / (_camera.Zoom + 1),
                    cameraFixedWidthAndHeight.TargetHeight / (_camera.Zoom + 1)
                ) / 2;
                
                var min = new Vector2(_bounds.Value.Left, _bounds.Value.Top) + viewSize;
                var max = new Vector2(_bounds.Value.Right, _bounds.Value.Bottom) - viewSize;

                newPos = Vector2.Clamp(newPos, min, max);
            }

            _camera.Position = newPos + _shakeOffset;
        }
        else
        {
            _camera.Position = Position + _shakeOffset;
        }

        // Smooth zoom
        _camera.Zoom = Tween.StepTo(_camera.Zoom, _targetZoom, _zoomSpeed, deltaTime);

        // Update shake
        if (_shakeIntensity > 0.01f)
        {
            _shakeOffset = new Vector2(
                Random.Shared.NextSingle() * 2 - 1,
                Random.Shared.NextSingle() * 2 - 1
            ) * _shakeIntensity;

            _shakeIntensity = Tween.StepTo(_shakeIntensity, 0f, _shakeDecay, deltaTime);
        }
        else
        {
            _shakeOffset = Vector2.Zero;
        }
    }

    protected override void OnDispose()
    {
        // No-op for now
    }
}