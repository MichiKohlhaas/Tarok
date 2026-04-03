using Tarok.Enums;
using Tarok.Errors;
using System.Collections.Generic;

namespace Tarok;

internal class Parser()
{
    private readonly Dictionary<Token, List<Token>> _slots = new();
    private readonly Dictionary<(int row, int col), Branch> _branches = new();
    private readonly List<TarokError> _errors = [];
    private List<Token> _trueBranch = [];
    private List<Token> _falseBranch = [];
    private readonly HashSet<int> _skipRows = [];
    private readonly List<Token> _executionTokens = [];
    private int _counter;
    
    
    internal ParsedProgram Parse(List<Token> tokens)
    {
        ClearDataStructures();
        var tokensByRow = tokens.GroupBy(t => t.Row)
                                                     .ToDictionary(k => k.Key, k => k.OrderBy(x => x.Column).ToList());

        while (_counter < tokens.Count)
        {
            var token = tokens[_counter];

            if (_skipRows.Contains(token.Row))
            {
                // handle nested Magician cards, fool blocks, etc.
                _counter++;
            }

            // TODO: for future, in case where fool block ends but there are more tokens on the same row as part of branch, do something.
            if (IsFoolUpright(token))
            {
                ParseFoolBlocks(tokens);    
                //_skipRows.Add(token.Row);
            }
            else if (IsMagician(token) && !_skipRows.Contains(token.Row))
            {
                if (tokensByRow[token.Row + 1].Find(x => x.Type == TokenEnum.EOF) is not null || token.Row - 1 < 0)
                {
                    _errors.Add(new TarokParseError($"Parser encountered Card branching out of grid range at: {token.Row}-{token.Column}.", token));
                    _counter++;
                    continue;
                }
                var trueBranch = tokensByRow[token.Row - 1];
                var falseBranch = tokensByRow[token.Row + 1];
                
                _branches[(token.Row, token.Column)] = new Branch(trueBranch, falseBranch);
                _executionTokens.Add(token);
                _skipRows.Add(token.Row - 1);
                _skipRows.Add(token.Row + 1);
                _counter++;
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
        _skipRows.Clear();
        _trueBranch.Clear();
        _falseBranch.Clear();
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