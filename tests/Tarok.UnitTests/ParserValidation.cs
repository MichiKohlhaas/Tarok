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
}