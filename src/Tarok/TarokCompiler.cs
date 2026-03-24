using Tarok.Errors;

namespace Tarok;

public class TarokCompiler
{
    private readonly Lexer _lexer = new();
    private readonly Parser _parser = new();
    private readonly Executor _executor;

    public TarokCompiler()
    {
        var evaluator = new Evaluator();
        _executor = new Executor(evaluator);
    }

    public IReadOnlyList<TarokError> Compile(string[,] grid)
    {
        var tokens = _lexer.ScanGrid(grid);
        if (_lexer.Errors.Count > 0) return _lexer.Errors;
        
        var parsedProgram = _parser.Parse(tokens);
        if (parsedProgram.Errors.Count > 0) return parsedProgram.Errors;

        return _executor.Execute(parsedProgram);
    }

    public bool IsValidToken(string token)
    {
        return _lexer.IsValidToken(token);
    }
}