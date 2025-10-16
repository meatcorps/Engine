using System.Numerics;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Particles;
using Meatcorps.Game.KillTheSkulls.GameObjects.Abstractions;
using Meatcorps.Game.KillTheSkulls.Particles;
using Raylib_cs;

namespace Meatcorps.Game.KillTheSkulls.GameObjects;

public class ScoreEmitter: ResourceGameObject
{
    private ParticleSystemBuilder _scoreEmitter = null!;
    private ParticleSystemBuilder _negativeEmitter = null!;


    protected override void OnInitialize()
    {
        base.OnInitialize();
        Layer = 12;
        Camera = CameraLayer.UI;
        _scoreEmitter = ScoreParticle.GenerateParticleSystem(new Color(0, 255, 255), Fonts.GetFont());
        _negativeEmitter = ScoreParticle.GenerateParticleSystem(new Color(255, 0, 0), Fonts.GetFont());
    }

    protected override void OnUpdate(float deltaTime)
    {
        _scoreEmitter.Update(deltaTime);
        _negativeEmitter.Update(deltaTime);
    }
    
    protected override void OnDraw()
    {
        _scoreEmitter.Draw();
        _negativeEmitter.Draw();
    }

    public void ShowScore(Vector2 position, string text)
    {
        if (!DemoMode)
            _scoreEmitter.Emit(1, position, new Vector2(0, -10), text);
    } 
    public void Negative(Vector2 position, string text)
    {
        if (!DemoMode)
            _negativeEmitter.Emit(1, position, new Vector2(0, -10), text);
    } 

    protected override void OnDispose()
    {
    }
}