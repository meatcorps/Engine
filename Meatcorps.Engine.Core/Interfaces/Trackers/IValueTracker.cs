namespace Meatcorps.Engine.Core.Interfaces.Trackers;

public interface IValueTracker
{
    void Change<T>(string name, T value);
    T Get<T>(string name);
}