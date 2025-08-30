using Meatcorps.Engine.AsciiScript.Data;
using Meatcorps.Engine.AsciiScript.Enums;
using Meatcorps.Engine.AsciiScript.Interfaces;

namespace Meatcorps.Engine.AsciiScript.Services;

public class AsciiScriptParser : IDisposable
{
    public ScriptParserState State { get; set; } = ScriptParserState.Idle;
    public float DeltaTime { get; private set; }
    public Dictionary<string, Func<IAsciiScriptCommand>> _commandMap { get; } = new();
    
    private List<IAsciiScriptCommand> _templateCommands = new();
    private List<IAsciiScriptCommand> _executeCommands  = new();
    private List<AsciiScriptItem> _commandParameters  = new();
    private AsciiScriptReader? _reader;
    private bool _isLoaded;
    private bool _isDisposed;
    private int _lineNumber;
    private int _runs;
    public AsciiScriptParser Register(Func<IAsciiScriptCommand> command)
    {
        var commandInstance = command();
        
        if (_commandMap.ContainsKey(commandInstance.Command))
            throw new InvalidOperationException($"Command already registered: {commandInstance.Command}");
        
        _commandMap.Add(commandInstance.Command, command);
        _templateCommands.Add(commandInstance);
        return this;
    }

    public AsciiScriptParser Load()
    {
        if (_isLoaded) 
            return this;
        _isLoaded = true;
        
        _reader = new AsciiScriptReader(
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
    
    public void Parse(string path)
    {
        if (!_isLoaded)
            throw new Exception("Parser not loaded");
        
        State = ScriptParserState.Idle;
        
        _reader!.LoadFromFileAndParse(path);
        ClearCommands();
        while (_reader.ReadNext(out var currentCommand))
        {
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