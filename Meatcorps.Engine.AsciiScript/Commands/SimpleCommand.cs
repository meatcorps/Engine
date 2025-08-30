using System.Globalization;
using Meatcorps.Engine.AsciiScript.Data;
using Meatcorps.Engine.AsciiScript.Enums;
using Meatcorps.Engine.AsciiScript.Interfaces;
using Meatcorps.Engine.AsciiScript.Services;

namespace Meatcorps.Engine.AsciiScript.Commands;

public class SimpleCommand : IAsciiScriptCommand
{
    private readonly Action _action;
    public AsciiScriptItemType ScriptType => AsciiScriptItemType.Command;
    public string Command { get; }
    
    public SimpleCommand(string command, Action action)
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
    }

    public void Execute(AsciiScriptItem scriptItem, AsciiScriptParser parser, int runs)
    {
        _action();
    }
}