using Tarok.Errors;

namespace Tarok;

internal sealed class Executor(Evaluator evaluator)
{
    private Dictionary<(int row, int col), Token> GridLookup { get; set; } = [];
    public List<TarokError> Errors { get; set; }
    
    
    internal IReadOnlyList<TarokError> Execute(ParsedProgram parsedProgram)
    {
        evaluator.Slots = parsedProgram.Slots;
        GridLookup = parsedProgram.ExecutionTokens.ToDictionary(x => (x.Row, x.Column));
        Errors.AddRange(parsedProgram.Errors);
        
        return Errors;
    }
}