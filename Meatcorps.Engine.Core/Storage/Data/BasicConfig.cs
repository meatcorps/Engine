using Meatcorps.Engine.Core.Storage.Abstractions;

namespace Meatcorps.Engine.Core.Storage.Data;

public class BasicConfig: BaseConfig<BasicConfig>
{
    public static void Load()
    {
        _ = new BasicConfig();
    }
    
    protected override void DoRegisterDefaultValues()
    {
    }
    
    protected override BasicConfig Instance => this;
}