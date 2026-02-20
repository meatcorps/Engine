using Meatcorps.Engine.AsciiScript.Commands;
using Meatcorps.Engine.AsciiScript.Data;
using Meatcorps.Engine.AsciiScript.Enums;
using Meatcorps.Engine.AsciiScript.Interfaces;
// ReSharper disable SuspiciousTypeConversion.Global

namespace Meatcorps.Engine.AsciiScript.Services;

public class AsciiScriptParser : IDisposable
{
    public ScriptParserState State { get; set; } = ScriptParserState.Idle;
    public float DeltaTime { get; private set; }
    private Dictionary<string, Func<IAsciiScriptCommand>> _commandMap { get; } = new();
    
    private readonly List<IAsciiScriptCommand> _templateCommands = new();
    private readonly List<IAsciiScriptCommand> _executeCommands  = new();
    private readonly List<AsciiScriptItem> _commandParameters  = new();
    public AsciiScriptReader? Reader { get; private set; }
    private bool _isLoaded;
    private bool _isDisposed;
    private int _lineNumber;
    private int _runs;
    
    public AsciiScriptParser Register(Func<IAsciiScriptCommand> command)
    {
        var commandInstance = command();
        
        if (!_commandMap.TryAdd(commandInstance.Command, command))
            throw new InvalidOperationException($"Command already registered: {commandInstance.Command}");

        _templateCommands.Add(commandInstance);
        return this;
    }

    public AsciiScriptParser Load()
    {
        if (_isLoaded) 
            return this;
        _isLoaded = true;
        
        Reader = new AsciiScriptReader(
            _templateCommands
                .Where(x => x.ScriptType == AsciiScriptItemType.Block)
                .Select(x => x.Command).ToArray(),
            _templateCommands
                .Where(x => x.ScriptType == AsciiScriptItemType.Command)
                .Select(x => x.Command).ToArray(),
            _templateCommands
                .Where(x => x.ScriptType == AsciiScriptItemType.Variable)
                .Select(x => x.Command).ToArray());
        
        return this;
    }

    public void JumpToLine(int position)
    {
        if (_lineNumber < 0 || _lineNumber >= _executeCommands.Count)
            throw new Exception("Jump to line out of range");
        _lineNumber = position;
        Console.WriteLine($"Jump to line {_lineNumber}");
    }
    
    public void Parse(string path)
    {
        if (!_isLoaded)
            throw new Exception("Parser not loaded");
        
        State = ScriptParserState.Idle;
        
        Reader!.LoadFromFileAndParse(path);
        ClearCommands();
        while (Reader.ReadNext(out var currentCommand))
        {
            if (currentCommand.Type == AsciiScriptItemType.Goto)
            {
                var currentCommandValue = currentCommand.Value;
                var targetLineNumber = Reader.SearchAll(scriptItem => scriptItem.Value == currentCommandValue && scriptItem.Type == AsciiScriptItemType.GotoLabel);
                _commandParameters.Add(currentCommand);
                _executeCommands.Add(new JumpToLineCommand(targetLineNumber));
                continue;
            }

            if (currentCommand.Type == AsciiScriptItemType.ConditionElse)
            {
                var targetLineNumber = 0;
                Reader.SearchAfter((item, i) =>
                {
                    if (item.Type == AsciiScriptItemType.ConditionEnd)
                    {
                        targetLineNumber = i;
                        return true;
                    }
                    return false;
                });
                _commandParameters.Add(currentCommand);
                _executeCommands.Add(new JumpToLineCommand(targetLineNumber));
                continue;
            }

            if (currentCommand.Type is AsciiScriptItemType.GotoLabel or AsciiScriptItemType.ConditionEnd)
            {
                _commandParameters.Add(currentCommand);
                _executeCommands.Add(new NothingCommand());
                continue;
            }

            var accepted = false;
            foreach (var template in _templateCommands)
            {
                if (template.Accept(currentCommand) &&
                    template.ScriptType == currentCommand.Type &&
                    template.Command == currentCommand.Command)
                {
                    var instance = _commandMap[template.Command]();
                    instance.Initialize(currentCommand,this);
                    _executeCommands.Add(instance);
                    _commandParameters.Add(currentCommand);
                    accepted = true;
                    Console.WriteLine($"Command accepted: {currentCommand.Command}({currentCommand.Type.ToString()}) On line number {currentCommand.LineNumber}");
                }
            }
            if (!accepted)
                Console.WriteLine($"WARNING Command not accepted: {currentCommand.Command}({currentCommand.Type.ToString()}) On line number {currentCommand.LineNumber}");
        }
        
        State = ScriptParserState.Running;
    }

    private void ClearCommands()
    {
        _commandParameters.Clear();
        foreach (var command in _executeCommands)
        {
            if (command is IDisposable disposable)
                disposable.Dispose();
        }
        _executeCommands.Clear();
    }

    public void Reset()
    {
        _lineNumber = 0;
        State = ScriptParserState.Running;
    }

    public void Update(float deltaTime)
    {
        if (State == ScriptParserState.Idle)
            return;
        
        if (_lineNumber >= _executeCommands.Count)
        {
            State = ScriptParserState.Idle;
            return;
        }

        DeltaTime = deltaTime;

        if (State == ScriptParserState.Paused)
            HandleCommand();
        
        while (_lineNumber < _executeCommands.Count && State == ScriptParserState.Running)
            HandleCommand();
    }

    private void HandleCommand()
    {
        var currentCommand = _executeCommands[_lineNumber];
        
        currentCommand.Execute(_commandParameters[_lineNumber], this, _runs);


        if (State == ScriptParserState.Running)
        {
            _lineNumber++;
            _runs = 0;
        } else if (State == ScriptParserState.Paused)
            _runs++;
    }


    public void Dispose()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;
        
        ClearCommands();
        foreach (var command in _templateCommands)
            if (command is IDisposable disposable)
                disposable.Dispose();
        
        _commandMap.Clear();
        _templateCommands.Clear();
    }
}