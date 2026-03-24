using Tarok.Errors;

namespace Tarok;

internal sealed class Executor(Evaluator evaluator)
{
    internal IReadOnlyList<TarokError> Execute(ParsedProgram parsedProgram)
    {
        evaluator.Slots = parsedProgram.Slots;
        
        return [];
    }
}