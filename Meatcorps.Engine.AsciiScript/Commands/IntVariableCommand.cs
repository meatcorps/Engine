using System.Globalization;
using Meatcorps.Engine.AsciiScript.Data;
using Meatcorps.Engine.AsciiScript.Enums;
using Meatcorps.Engine.AsciiScript.Interfaces;
using Meatcorps.Engine.AsciiScript.Services;

namespace Meatcorps.Engine.AsciiScript.Commands;

public class IntVariableCommand : IAsciiScriptCommand
{
    private readonly Action<int> _action;
    public AsciiScriptItemType ScriptType => AsciiScriptItemType.Variable;
    public string Command { get; }
    public int Data { get; private set; }
    
    public IntVariableCommand(string command, Action<int> action)
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
        if (!int.TryParse(scriptItem.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var data))
            throw new FormatException($"Invalid DELAY value: '{scriptItem.Value}'");
        Data = data;
    }

    public void Execute(AsciiScriptItem scriptItem, AsciiScriptParser parser, int runs)
    {
        _action(Data);
    }
}