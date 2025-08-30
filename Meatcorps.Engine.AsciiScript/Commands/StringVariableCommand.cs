using Meatcorps.Engine.AsciiScript.Data;
using Meatcorps.Engine.AsciiScript.Enums;
using Meatcorps.Engine.AsciiScript.Interfaces;
using Meatcorps.Engine.AsciiScript.Services;

namespace Meatcorps.Engine.AsciiScript.Commands;

public class StringVariableCommand : IAsciiScriptCommand
{
    private readonly Action<string> _action;
    public AsciiScriptItemType ScriptType => AsciiScriptItemType.Variable;
    public string Command { get; }
    public string Data { get; private set; } = string.Empty;
    
    public StringVariableCommand(string command, Action<string> action)
    {
        _action = action;
        Command = command;
    }
    
    public bool Accept(AsciiScriptItem scriptItem)
    {
        return true;
    }

    public void Initialize(AsciiScriptItem scriptItem, AsciiScriptParser parser)
    {
        Data = scriptItem.Value;
    }

    public void Execute(AsciiScriptItem scriptItem, AsciiScriptParser parser, int runs)
    {
        _action(Data);
    }
}