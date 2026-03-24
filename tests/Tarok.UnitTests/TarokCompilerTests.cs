using Tarok.UnitTests.Helpers;

namespace Tarok.UnitTests;

public class TarokCompilerTests
{
    private TarokCompiler _compiler;
    
    [SetUp]
    public void Setup()
    {
        _compiler = new TarokCompiler();
    }

    [Test]
    public void Compiler_IntegrationTest_FoolGridParsed_ShouldReturnTrue()
    {
        var foolSpread = TestDataBuilder.CreateFoolSpread();
        var result = _compiler.Compile(foolSpread);
        Assert.That(result, Has.Count.EqualTo(0));
    }
}