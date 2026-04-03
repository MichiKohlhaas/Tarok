using Tarok.Enums;
using Tarok.Errors;
using System.Collections.Generic;

namespace Tarok;

internal class Parser()
{
    private readonly Dictionary<Token, List<Token>> _slots = new();
    private readonly Dictionary<(int row, int col), Branch> _branches = new();
    private readonly List<TarokError> _errors = [];
    private readonly HashSet<(int row, int col)> _skipCoordinates = [];
    private readonly List<Token> _executionTokens = [];
    private int _counter;
    
    
    internal ParsedProgram Parse(List<Token> tokens)
    {
        ClearDataStructures();
        var tokensByRow = tokens.GroupBy(t => t.Row)
            .ToDictionary(k => k.Key, k => k.OrderBy(x => x.Column).ToList());
        
        var magicianTokens = tokens.Where(t => t.Arcana is MajorArcana { Card: Trump.Magician })
            .ToList();
        magicianTokens.ForEach(m => ParseBranch(m, tokensByRow));
        
        while (_counter < tokens.Count)
        {
            var token = tokens[_counter];

            if (_skipCoordinates.Contains((token.Row, token.Column)))
            {
                _counter++;
                continue;
            }
            if (IsFoolUpright(token))
            {
                ParseFoolBlocks(tokens);    
            }
            else
            {
                _executionTokens.Add(token);
                _counter++;
            }
        }
        
        return new ParsedProgram(_executionTokens, _slots, _errors, _branches);
    }

    private void ClearDataStructures()
    {
        _counter = 0;
        _slots.Clear();
        _errors.Clear();
        _skipCoordinates.Clear();
        _executionTokens.Clear();
    }

    private void ParseFoolBlocks(List<Token> tokens)
    {
        var tempTokens = new List<Token>();
        // skipping the Fool card at tokens[index].
        var nextToken = tokens[++_counter];
        while (_counter < tokens.Count && !IsFoolReversed(nextToken))
        {
            tempTokens.Add(nextToken);
            _counter++;
            if (_counter >= tokens.Count)
            {
                _errors.Add(new TarokParseError("Unclosed Fool block.", tokens[_counter - 1]));
                return;
            }
            nextToken = tokens[_counter];
        }
                            
        // @0 found
        // store in memory slot if next card is Ace P,W,C
        var aceToken = tokens.ElementAtOrDefault(_counter + 1);
        if (aceToken is not null && aceToken.Arcana is not MinorArcana { Rank: 1 })
        {
            _errors.Add(new TarokParseError("Fool block closed without assigning to memory slot.", aceToken));
            _counter++;
            return;
        }
        // case: counter + 1 = index out of bounds
        if (aceToken is null)
        {
            _errors.Add(new TarokParseError("Unclosed Fool block.", tokens[_counter]));
            _counter++;
            return;
        }
        // if the Ace slot is already occupied, overwrite it (intentional).
        _slots[tokens[++_counter]] = tempTokens;
    }

    private void ParseBranch(Token magician, Dictionary<int, List<Token>> tokensByRow)
    {
        var magicianRow = magician.Row;
        var magicianCol = magician.Column;
        
        if (tokensByRow[magicianRow + 1].Find(x => x.Type == TokenEnum.EOF) is not null || magicianRow - 1 < 0)
        {
            _errors.Add(new TarokParseError($"Parser encountered Card branching out of grid range at: {magicianRow}-{magicianCol}.", magician));
            return;
        }

        var trueBranchRow = magicianRow - 1;
        var falseBranchRow = magicianRow + 1;

        var trueBranchTokens = tokensByRow.GetValueOrDefault(trueBranchRow, [])
            .Where(t => t.Column >= magicianCol)
            .ToList();
        var falseBranchTokens = tokensByRow.GetValueOrDefault(falseBranchRow, [])
            .Where(t => t.Column >= magicianCol)
            .ToList();
        
        _branches[(magicianRow, magicianCol)] = new Branch(trueBranchTokens, falseBranchTokens);
        _skipCoordinates.Add((trueBranchRow, magicianCol));
        _skipCoordinates.Add((falseBranchRow, magicianCol));
    }

    private static bool IsFoolUpright(Token token)
    {
        return token.Arcana is MajorArcana { Card: Trump.Fool, IsReversed: false };
    }

    private static bool IsFoolReversed(Token token)
    {
        return token.Arcana is MajorArcana { Card: Trump.Fool, IsReversed: true };
    }

    private static bool IsMagician(Token token)
    {
        return token.Arcana is MajorArcana { Card: Trump.Magician };
    }
}