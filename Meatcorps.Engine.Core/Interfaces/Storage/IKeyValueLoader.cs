namespace Meatcorps.Engine.Core.Interfaces.Storage;

public interface IKeyValueLoader<T>
{
    void GetData(IKeyValueDatabase<T> target);
}