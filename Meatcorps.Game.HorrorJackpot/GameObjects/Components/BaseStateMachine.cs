using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Core.Interfaces.Components;
using Meatcorps.Game.HorrorJackpot.Shaders;

namespace Meatcorps.Game.HorrorJackpot.GameObjects.Components;

public abstract class BaseStateMachine : IGameComponent
{
    protected MainGameObject Target { get; } 
    protected abstract GameInternalState TargetState { get; }
    protected DrumRenderer DrumRenderer => Target.DrumRenderer;
    protected ArcadeGame GameInfo => Target.GameInfo;

    protected string Text
    {
        get => Target.Text;
        set => Target.Text = value;
    } 
    
    public BaseStateMachine(MainGameObject mainGameObject)
    {
        Target = mainGameObject;
    }
    
    public void Initialize()
    {
        
    }

    public void PreUpdate(float deltaTime)
    {
        
    }

    public void Update(float deltaTime)
    {
        if (Target.InternalState == TargetState)   
            UpdateState(deltaTime);
    }
    
    protected abstract void UpdateState(float deltaTime);

    public void LateUpdate(float deltaTime)
    {
        
    }

    public void Draw()
    {
        
    }
}