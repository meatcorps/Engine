using System.Numerics;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.R3d.Interfaces;
using R3D_cs;

namespace Meatcorps.Engine.RayLib.R3d.Camera;

public class Camera3D : ICamera3D
{
    public Vector2 Position
    {
        get => new Vector2(_nativeCamera.Position.X, _nativeCamera.Position.Y);
        set
        {
            _nativeCamera.Position.X = value.X;
            _nativeCamera.Position.Y = value.Y;
        }
    }

    public float Zoom { get; set; }

    public static Camera3D DefaultOrthographic()
    {
        return new Camera3D(true, 10);
    }
    
    
    public static Camera3D DefaultPerspective()
    {
        return new Camera3D(false);
    }
    
    public Camera3D(bool orthographic, double cameraViewAngle = 60, double cameraNearPlane = 0.05, double cameraFarPlane = 4000)
    {
        
        _nativeCamera = new R3D_cs.Camera
        {
            Position = Vector3.Zero,
            Rotation = Quaternion.Identity,
            Projection = orthographic
                ? Projection.Orthographic
                : Projection.Perspective,
            Fovy = cameraViewAngle,
            NearPlane = cameraNearPlane,
            FarPlane = cameraFarPlane,
            CullMask = Layer.All
        };
    }

    public void StartCamera()
    {
        // Nop
    }

    public void EndCamera()
    {
        // Nop
    }

    public void UpdateRaylibCamera(ref Raylib_cs.Camera3D raylibCamera)
    {
        _nativeCamera = R3D.CameraFromRL(raylibCamera);
    }

    public void Update(float deltaTime, IRenderTargetStrategy renderTargetStrategy)
    {
        if (!Target3D.HasValue)
            return;
        
        R3D.CameraLookAt(ref _nativeCamera, Target3D.Value, Target3DUpDirection);
    }

    public Vector3 Position3D
    {
        get => _nativeCamera.Position;
        set => _nativeCamera.Position = value;
    }

    public Quaternion Rotation
    {
        get => _nativeCamera.Rotation;
        set => _nativeCamera.Rotation = value;
    }

    public Vector3? Target3D { get; set; } = null;

    public Vector3 Target3DUpDirection { get; set; } = Vector3.UnitY;

    public bool Orthographic
    {
        get => _nativeCamera.Projection == Projection.Orthographic;
        set => _nativeCamera.Projection = value ? Projection.Orthographic : Projection.Perspective;
    }

    private R3D_cs.Camera _nativeCamera;

    public ref R3D_cs.Camera Native => ref _nativeCamera;
}