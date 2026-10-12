using System.Numerics;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.R3d.Data;
using Meatcorps.Engine.RayLib.R3d.Examples.GameObjects;
using Meatcorps.Engine.RayLib.R3d.Examples.Resources;
using Meatcorps.Engine.RayLib.R3d.Resources;
using R3D_cs;
using Raylib_cs;
using Camera3D = Meatcorps.Engine.RayLib.R3d.Camera.Camera3D;

namespace Meatcorps.Engine.RayLib.R3d.Examples.Scenes;

public class MainScene: BaseScene
{
    private Camera3D _camera = null!;
    private Raylib_cs.Camera3D _nativeCamera = new ()
    {
        Position = new Vector3(0, 1.5f, 3.0f),
        Target = new Vector3(0, 0.75f, 0.0f),
        Up = Vector3.UnitY,
        FovY = 60
    };

    protected override void OnInitialize()
    {
        GlobalObjectManager.ObjectManager.Get<R3dSettings>()!.EnvironmentChange((ref env) =>
        {
            env.Tonemap.Mode = Tonemap.Filmic;
            env.Tonemap.Exposure = 0.2f;
        });
        GlobalObjectManager.ObjectManager.Get<R3dCubemap>("Assets/indoor.hdr")!.SetAsSkybox();
        
        _camera = (Camera3D)GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()!.Camera!;
        _camera.Position3D = new Vector3(0, 1.5f, 3.0f);
        _camera.Target3D = new Vector3(0, 0.75f, 0.0f);
        AddGameObject(new MainGameObject());
    }

    protected override void OnUpdate(float deltaTime)
    {
        Raylib.UpdateCamera(ref _nativeCamera, CameraMode.Orbital);
        _camera.UpdateRaylibCamera(ref _nativeCamera);
    }

    protected override void OnDispose()
    {
    }
}