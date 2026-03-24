using Tarok.Errors;

namespace Tarok;

internal sealed class ParsedProgram(List<Token> executionTokens, Dictionary<Token, List<Token>> slots, List<TarokError> errors)
{
    internal List<Token> ExecutionTokens { get; } = executionTokens;
    
    internal Dictionary<Token, List<Token>> Slots { get; } = slots;
    
    internal List<TarokError> Errors { get; } =  errors;
}