using Tarok.Enums;
using Tarok.Errors;
using Tarok.Utility;

namespace Tarok;

/// Grammar
/// expression := literal | unary | operator | grouping
/// 
/// token      := major | minor | reversed
/// 
/// reversed   := '@' (major | minor)
/// 
/// major      := '0' | roman
/// 
/// roman      := 'I' | 'II' | 'III' | 'IV' | 'V' | 'VI' | 'VII' | 'VIII'
///     | 'IX' | 'X' | 'XI' | 'XII' | 'XIII' | 'XIV' | 'XV' | 'XVI'
///     | 'XVII' | 'XVIII' | 'XIX' | 'XX' | 'XXI'
///
/// grouping    := 0 | expression | @0
/// 
/// minor      := rank suit
/// 
/// rank       := '1' | '2' | '3' | '4' | '5' | '6' | '7' | '8' | '9'
///     | '10' | '11' | '12' | '13' | '14'
/// 
/// suit       := 'S' | 'W' | 'P' | 'C'
///
/// literal    := '2-14C' | '1-14P' 
/// 
/// unary      := ( ¬ | ¬¬ ) 
///
/// ¬/¬¬       := 5W/@5W
///
/// operator   := OR | NOR | ADD | SUBTRACT | EQUALS | NOT EQUALS
///            | MULTIPLY | DIVIDE | GREATER THAN | LESS THAN
///            | AND | NAND
///
/// OR/NOR     := 2W/@2W, usw.
internal class Evaluator()
{
    internal Dictionary<Token, List<Token>>? Slots { get; set; }
    private readonly HashSet<Token> _evaluatingToken = [];

    internal object EvaluateSlots(Token aceToken)
    {
        if (Slots?.TryGetValue(aceToken, out var slot) != false)
            throw new TarokRuntimeError($"Memory slot {aceToken} has not been defined");
        return Evaluate(slot!);
    }
    
    
    private object Evaluate(List<Token> tokens)
    {
        Stack<object> stack = new();
        
        foreach (var token in tokens)
        {
            switch (GetRole(token))
            {
                case Role.NumericalLiteral:
                    stack.Push(GetNumericValue(token));
                    break;
                case Role.CharacterLiteral:
                    stack.Push(GetCharacterValue(token));
                    break;
                case Role.Operator:
                    ApplyOperator(token, stack);
                    break;
                case Role.MemorySlot:
                    if (!_evaluatingToken.Add(token))
                        throw new TarokRuntimeError("Circular reference detected");
                    stack.Push(Evaluate(Slots[token]));
                    _evaluatingToken.Remove(token);
                    break;
                case Role.ConditionalMarker:
                case Role.Unknown:
                default:
                    break;
            }
        } 
        return Pop(stack);
    }

    private static void ApplyOperator(Token token, Stack<object> stack)
    {
        if (IsUnary(token))
        {
            var operand = Pop(stack);
            stack.Push(ApplyUnary(token, operand));
        }
        else
        {
            var right = Pop(stack);
            var left = Pop(stack);
            stack.Push(ApplyBinary(token, left, right));
        }
    }

    /*
     * 2W = OR, @2W = NOR
       3W = ADD, @3W = SUBTRACT
       4W = EQUALS, @4W = !=
       6W = MULTIPLY, @6W = DIVIDE
       7W = GREATER THAN, @7W = LESS THAN
       9W = AND, @9W = NAND
     */
    private static object ApplyBinary(Token token, object left, object right)
    {
        return (token.Arcana as MinorArcana)!.Rank switch
        {
            2 => ApplyLogicalOr(token.Arcana.IsReversed, left, right),
            3 => ApplyAddSub(token.Arcana.IsReversed, left, right),
            6 => ApplyMultiplyDivide(token.Arcana.IsReversed, left, right),
            4 => token.Arcana.IsReversed ? !left.Equals(right) : left.Equals(right),
            7 => ApplyComparison(token.Arcana.IsReversed, left, right),
            9 => ApplyLogicalAnd(token.Arcana.IsReversed, left, right),
            _ => throw new NotImplementedException()
        };
    }

    private static object ApplyLogicalAnd(bool isReversed, object left, object right)
    {
        return left switch
        {
            int l when right is int r => isReversed ? ~(l & r) : l & r,
            char lc when right is char rc => isReversed ? ~(lc & rc) : lc & rc,
            bool lb when right is bool rb => isReversed ? !(lb && rb) : lb && rb,
            _ => throw new TarokRuntimeError(
                $"Malformed binary operator. Logical AND cannot be performed on: {left} {right}")
        };
    }

    private static object ApplyComparison(bool isReversed, object left, object right)
    {
        return left switch
        {
            int l when right is int r => isReversed ? l < r : l > r,
            char lc when right is char rc => isReversed ? lc < rc : lc > rc, 
            _ => throw new TarokRuntimeError($"Comparison operator not supported by {left} and {right}")
        };
    }

    private static object ApplyLogicalOr(bool isReversed, object left, object right)
    {
        return left switch
        {
            bool l when right is bool r => isReversed ? !(l || r) : l || r,
            int ll when right is int rr => isReversed ? ~(ll | rr) : ll | rr,
            char lc when right is char rc => isReversed ? ~(lc | rc) : lc | rc,
            _ => throw new TarokRuntimeError(
                $"Malformed binary. {left} cannot be logical {(isReversed ? "NOR" : "OR")} with {right}")
        };
    }

    private static int ApplyAddSub(bool isReversed, object left, object right)
    {
        if (left is not int li || right is not int ri) throw new TarokRuntimeError(
            $"Malformed binary. Arithmetic cannot be performed on {left} and {right}");
        return isReversed ? li - ri : li + ri;
    }

    private static int ApplyMultiplyDivide(bool isReversed, object left, object right)
    {
        if (left is not int li || right is not int ri) throw new TarokRuntimeError(
            $"Malformed binary. Arithmetic cannot be performed on {left} and {right}");
        if (isReversed && ri == 0)
            throw new TarokRuntimeError("Division by zero");
        return isReversed ? li / ri : li * ri;
    }


    private static object ApplyUnary(Token token, object operand)
    {
        return operand switch
        {
            bool boolOp => token.Arcana is { IsReversed : true } ? boolOp : !boolOp,
            int intOp => token.Arcana is { IsReversed: true } ? intOp : ~intOp,
            _ => token.Arcana is { IsReversed : true } ? IsTruthy(operand) : !IsTruthy(operand)  
        };
    }

    private static bool IsTruthy(object? o)
    {
        return o switch
        {
            null => false,
            bool b => b,
            _ => true
        };
    }

    private static bool IsUnary(Token token)
    {
        return token.Arcana is MinorArcana { Rank: 5 };
    }

    private static int GetNumericValue(Token token)
    {
        return (token.Arcana as MinorArcana)!.Rank;
    }

    private static char GetCharacterValue(Token token)
    {
        return (token.Arcana as MinorArcana)!.Rank.ToLetter(token.Arcana.IsReversed);
    }

    private static Role GetRole(Token token) => token.Arcana switch
    {
        MinorArcana(_, Suit.Pentacles, _) => Role.NumericalLiteral,
        MinorArcana(>= 2, Suit.Cups, _) => Role.CharacterLiteral,
        MinorArcana(1, _, _) => Role.MemorySlot,
        MinorArcana(_, Suit.Wands, _) => Role.Operator,
        MajorArcana(Trump.Justice, _) => Role.ConditionalMarker,
        _ => Role.Unknown
    };

    private static object Pop(Stack<object> stack)
    {
        if (stack.Count == 0)
            throw new TarokRuntimeError("Malformed expression: no operands");
        return stack.Pop();
    }
}