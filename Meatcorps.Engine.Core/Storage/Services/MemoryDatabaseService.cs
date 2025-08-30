using Meatcorps.Engine.Core.Interfaces.Storage;

namespace Meatcorps.Engine.Core.Storage.Services;

public class MemoryDatabaseService : Dictionary<string, object>, IKeyValueDatabase<string>
{
    public bool Dirty { get => false; set => _ = value; }
}