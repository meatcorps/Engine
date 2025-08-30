namespace Meatcorps.Engine.Core.Interfaces.Services;

public interface IBackgroundService
{
    void PreUpdate(float deltaTime);
    void Update(float deltaTime);
    void LateUpdate(float deltaTime);
}