using System.Runtime.CompilerServices;
using Tarok.Enums;
using Tarok.UnitTests.Helpers;

namespace Tarok.UnitTests;

public class ParserValidation
{
    private Parser _parser;
    
    [SetUp]
    public void Setup()
    {
        _parser = new Parser();
    }

    [Test]
    public void ParseFoolBlock_OneBlock_ShouldReturnTrue()
    {
        List<Token> tokens = TestDataBuilder.CreateFoolBlock();
        var result = _parser.Parse(tokens);
        Assert.That(result.Slots, Has.Count.EqualTo(1));
    }

    [Test]
    public void ParseFoolBlock_TwoBlocks_ShouldReturnTrue()
    {
        var tokens1 = TestDataBuilder.CreateFoolBlock();
        var tokens2 = TestDataBuilder.CreateFoolBlock();
        // in case the Ace cards are the same
        if (tokens1[^1].Equals(tokens2[^1]))
        {
            tokens2 = TestDataBuilder.CreateFoolBlock();
        }
        Console.WriteLine(tokens1[^1].Arcana);
        Console.WriteLine(tokens2[^1].Arcana);
        var tokens = tokens1.Concat(tokens2).ToList();
        var parsedProgram = _parser.Parse(tokens);
        Assert.That(parsedProgram.Slots, Has.Count.EqualTo(2));
    }
    
    [Test]
    public void ParseFoolBlock_NoClosingFool_ShouldReturnError()
    {
        var tokens = TestDataBuilder.CreateFoolBlock();
        var parsedProgram = _parser.Parse(tokens[..^2]);
        Assert.That(parsedProgram.Errors, Has.Count.EqualTo(1));
    }
    
    [Test]
    public void ParseFoolBlock_ClosedButNoAce_ShouldReturnError()
    {
        var tokens = TestDataBuilder.CreateFoolBlock();
        var parsedProgram = _parser.Parse(tokens[..^1]);
        Assert.That(parsedProgram.Errors, Has.Count.EqualTo(1));
    }

    [Test]
    public void ParseFoolBlock_OverwriteAce_ShouldReturnTrue()
    {
        var tokens = TestDataBuilder.CreateFoolBlock();
        tokens = tokens.Concat(tokens).ToList();
        var parsedProgram = _parser.Parse(tokens);
        Assert.That(parsedProgram.Slots, Has.Count.EqualTo(1));
    }

    [Test]
    public void ParseFoolBlock_ExecutionTokensContainNoFool_ShouldReturnTrue()
    {
        var foolSpread = TestDataBuilder.CreateFoolSpread();
        var lexer = new Lexer();
        var tokens = lexer.ScanGrid(foolSpread);
        var parsedProgram = _parser.Parse(tokens);
        
        var expectedTokens = TestDataBuilder.ExpectedTokens();
        Assert.That(parsedProgram.ExecutionTokens, Is.EqualTo(expectedTokens));
    }

    [Test]
    public void ParseMagicianBlock_TrueFalseBranch_ShouldReturnTrue()
    {
        var magicianSpread = TestDataBuilder.CreateMagicianBranchSpread();
        var lexer = new Lexer();
        var tokens = lexer.ScanGrid(magicianSpread);
        var parsedProgram = _parser.Parse(tokens);
        Assert.That(parsedProgram.Branches, Has.Count.EqualTo(1));
    }
}