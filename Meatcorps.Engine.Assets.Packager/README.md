# Meatcorps.Engine.Assets.Packager

**Meatcorps.Engine.Assets.Packager** This is the asset packager system

Add this to your csproj to enable asset packaging
```xml 
    <PropertyGroup>
        <MSBuildWarningsAsMessages>High</MSBuildWarningsAsMessages>
        <ConsoleLoggerParameters>verbosity=detailed</ConsoleLoggerParameters>
    </PropertyGroup>
    
    <UsingTask TaskName="AssetPackageTask"
               AssemblyFile="$(ProjectDir)\bin\$(Configuration)\net8.0\Meatcorps.Engine.Assets.Packager.dll" />

    <Target Name="AssetPackageTask"
            AfterTargets="Build"
            Condition="'$(Configuration)' == 'Release'">

        <AssetPackageTask
                GameAssemblyPath="$(TargetPath)"
                ProjectDirectory="$(ProjectDir)"
                OutputPath="$(OutputPath)" />
    </Target>
```
Add AssetConfig.cs to your project
```csharp
using Meatcorps.Engine.Assets.Interfaces;
using Meatcorps.Engine.Core.Interfaces.Security;
using Meatcorps.Engine.Core.Security.Sinks;

namespace YourAmazingGame;

public class AssetConfig: IAssetPackagerConfig
{
    public IEncryptDecryptSink EncryptDecryptSink => new AesResourceSink("CHANGETHIS!!!!");
    public List<AssetPack> AssetPacks => new List<AssetPack>(
    [
        new AssetPack
        {
            EncryptDecryptSink = null,
            Name = "Asset.pak",
            RelativePath = "Assets"
        }
    ]);
}
```

Add this to your project.cs
```csharp
RaylibAssetsModule.Load(new AssetConfig());
```

Then when in release mode it package everything inside the Assets folder in to Assets.pak

## License

MIT License  
See `LICENSE` for details.