namespace Tarok.UnitTests;

public class Tests
{
    private Lexer _lexer;
    [SetUp]
    public void Setup()
    {
         _lexer = new Lexer();
    }

    [Test]
    public void IsValid_MajorArcanaToken_ShouldBeTrue()
    {
        const string justice = "   XI  ";
        var result = _lexer.IsValidToken(justice);
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValid_MajorArcanaToken_ShouldBeFalse()
    {
        const string devil = "XXXX";
        var result = _lexer.IsValidToken(devil);
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValid_MajorArcanaFool_ShouldBeTrue()
    {
        const string fool = "0";
        var result = _lexer.IsValidToken(fool);
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValid_MajorArcanaFool_ShouldBeFalse()
    {
        const string fool = "0W";
        var result = _lexer.IsValidToken(fool);
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValid_MajorArcanaReversed_ShouldBeTrue()
    {
        const string strength = "@VIII";
        var result = _lexer.IsValidToken(strength);
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValid_MinorArcanaToken_ShouldBeTrue()
    {
        const string fourPentacles = "4P";
        var result = _lexer.IsValidToken(fourPentacles);
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValid_MinorArcanaToken_WrongRank_ShouldBeFalse()
    {
        const string fifteenPentacles = "15P";
        var result = _lexer.IsValidToken(fifteenPentacles);
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValid_MinorArcanaToken_WrongSuit_ShouldBeFalse()
    {
        const string wrongSuit = "3Z";
        var result = _lexer.IsValidToken(wrongSuit);
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void ScanGrid_ListOfTokens_ShouldBeTrue()
    {
        
        var grid = Helpers.TestDataBuilder.CreateSpread();
        var result = _lexer.ScanGrid(grid);
        Assert.That(result, Is.Not.Empty);
        
    }
    
    [Test]
    public void ScanGrid_EmptyCells_ShouldBeOK()
    {
        var grid = Helpers.TestDataBuilder.CreateSpread();
        grid[0, 0] = null!;
        var result = _lexer.ScanGrid(grid);
        Assert.That(result, Is.Not.Empty);
    }
}