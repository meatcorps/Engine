namespace Meatcorps.Engine.Core.Interfaces.Components;

public interface IGameComponent
{
    void Initialize();
    void PreUpdate(float deltaTime);
    void Update(float deltaTime);
    void LateUpdate(float deltaTime);
    void Draw();
}