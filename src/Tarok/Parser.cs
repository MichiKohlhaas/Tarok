using Tarok.Enums;
using Tarok.Errors;

namespace Tarok;

internal class Parser()
{
    private readonly Dictionary<Token, List<Token>> _slots = new();
    private readonly List<TarokError> _errors = [];

    internal ParsedProgram Parse(List<Token> tokens)
    {
        _slots.Clear();
        _errors.Clear();
        
        var evaluationTokens = new List<Token>();
        var counter = 0;

        while (counter < tokens.Count)
        {
            var token = tokens[counter];

            if (IsFoolUpright(token))
            {
                ParseFoolBlocks(tokens, ref counter);    
            }
            else
            {
                evaluationTokens.Add(token);
                counter++;
            }
        }
        return new ParsedProgram(evaluationTokens, _slots, _errors);
    }

    private void ParseFoolBlocks(List<Token> tokens, ref int counter)
    {
        var tempTokens = new List<Token>();
        // skipping the Fool card at tokens[index].
        var nextToken = tokens[++counter];
        while (counter < tokens.Count && !IsFoolReversed(nextToken))
        {
            tempTokens.Add(nextToken);
            counter++;
            if (counter >= tokens.Count)
            {
                _errors.Add(new TarokParseError("Unclosed Fool block.", tokens[counter - 1]));
                return;
            }
            nextToken = tokens[counter];
        }
                            
        // @0 found
        // store in memory slot if next card is Ace P,W,C
        var aceToken = tokens.ElementAtOrDefault(counter + 1);
        if (aceToken is not null && aceToken.Arcana is not MinorArcana { Rank: 1 })
        {
            _errors.Add(new TarokParseError("Fool block closed without assigning to memory slot.", aceToken));
            return;
        }
        // case: counter + 1 = index out of bounds
        if (aceToken is null)
        {
            _errors.Add(new TarokParseError("Unclosed Fool block.", tokens[counter]));
            return;
        }
        // if the Ace slot is already occupied, overwrite it (intentional).
        _slots[tokens[++counter]] = tempTokens;
    }

    private static bool IsFoolUpright(Token token)
    {
        return token.Arcana is MajorArcana { Card: Trump.Fool, IsReversed: false };
    }

    private static bool IsFoolReversed(Token token)
    {
        return token.Arcana is MajorArcana { Card: Trump.Fool, IsReversed: true };
    }
}