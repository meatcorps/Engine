using Meatcorps.Engine.AsciiScript.Commands;
using Meatcorps.Engine.AsciiScript.Enums;
using Meatcorps.Engine.AsciiScript.Services;
using Meatcorps.Engine.RayLib.Abstractions;

namespace Meatcorps.Engine.RayLib.Examples.GameObjects;

public class AsciiScriptPlayground: BaseGameObject
{
    private AsciiScriptParser _script = null!;

    protected override void OnInitialize()
    {
        _script = new AsciiScriptParser();

        _script.Register(() => new StringVariableCommand("SAY", s => Console.WriteLine(s)));
        _script.Register(() => new ConditionCommand("Whatever", () => false));
        _script.Register(() => new SimpleCommand("STOP", () => _script.State = ScriptParserState.Idle));
        _script.Load();
        _script.Parse("Assets/AsciiScriptWithConditions.txt");
    }

    protected override void OnUpdate(float deltaTime)
    {
        _script.Update(deltaTime);
    }

    protected override void OnDispose()
    {
        
    }
}