namespace Meatcorps.Engine.RayLib.Interfaces;

public interface IMasterVolume
{
    string Name { get; }
    float MasterVolume { get; }
    void SetMasterVolume(float volume);
}