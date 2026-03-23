using Tarok.Errors;

namespace Tarok;

public class ParsedProgram(List<Token> executionTokens, Dictionary<Token, List<Token>> slots, List<TarokError> errors)
{
    public List<Token> ExecutionTokens { get; init; } = executionTokens;
    
    public Dictionary<Token, List<Token>> Slots { get; init; } = slots;
    
    public List<TarokError> Errors { get; init; } =  errors;
}