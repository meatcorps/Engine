using System.Numerics;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.R3d.Resources;
using R3D_cs;
using Raylib_cs;
using Mesh = R3D_cs.Mesh;
using Model = R3D_cs.Model;

namespace Meatcorps.Engine.RayLib.R3d.Examples.GameObjects;

public class MainGameObject: BaseGameObject
{
    private Mesh _plane;
    private Model _model;
    private AnimationLib _modelAnimation;
    private AnimationPlayer _modelPlayer;
    private InstanceBuffer _instances;
    private Light _light;
    private ShadowMap _shadow;

    protected override void OnInitialize()
    {
        
        _plane = R3D.GenMeshPlane(10, 10, 1, 1);
        var loader = GlobalObjectManager.ObjectManager.Get<R3dImportResource>("Assets/CesiumMan.glb")!;
        _model = loader.CreateModelInstance();
        _modelAnimation = loader.CreateAnimationLibInstance();
        _modelPlayer = R3D.LoadAnimationPlayer(_model.Skeleton, _modelAnimation);
        R3D.SetAnimationLoop(ref _modelPlayer, 0, true);
        R3D.PlayAnimation(ref _modelPlayer, 0);
        
        _instances = R3D.LoadInstanceBuffer(256, InstanceFlags.Position);
        var positions = R3D.MapInstances<Vector3>(_instances, InstanceFlags.Position);
        for (int z = 0; z < 16; z++) {
            for (int x = 0; x < 16; x++) {
                positions[z*16 + x] = new Vector3(x - 0.5f, 0, z - 0.5f);
            }
        }
        R3D.UnmapInstances(_instances, InstanceFlags.Position);
        
        _light = R3D.CreateDirLight(new Vector3(-1.0f, -1.0f, -1.0f), Color.White, 1);
        _light.Range = 10;

        _shadow = R3D.LoadShadowMap(LightType.Dir);
        _shadow.Softness = 2.0f;
    }

    protected override void OnUpdate(float deltaTime)
    {
        R3D.UpdateAnimationPlayer(ref _modelPlayer, deltaTime);
    }

    protected override void OnDraw()
    {
        R3D.PushLightEx(_light, _shadow, true);
        R3D.DrawMesh(_plane, R3D.MATERIAL_BASE, Vector3.Zero, 1.0f);
        R3D.DrawAnimatedModel(_model, _modelPlayer, Vector3.Zero, 1.25f);
        R3D.DrawAnimatedModelInstanced(_model, _modelPlayer, _instances, 256);
    }

    protected override void OnDispose()
    {
        R3D.UnloadShadowMap(_shadow);
        R3D.UnloadAnimationPlayer(_modelPlayer);
        R3D.UnloadAnimationLib(_modelAnimation);
        R3D.UnloadMesh(_plane);
        R3D.UnloadModel(_model, true);
        R3D.UnloadInstanceBuffer(_instances);
    }
}