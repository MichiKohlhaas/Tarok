using Tarok;

namespace TarotRepl;

internal class ReplEngine()
{
    private readonly Lexer _lexer = new();

    private void Evaluate(string line)
    {
        _lexer.Source = line;
        var tokens = _lexer.ScanTokens();
        foreach (var token in tokens)
        {
            Console.WriteLine(token);
        }
    }

    public void Print(string line)
    {
        Console.WriteLine(line);
    }

    public void Start()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line)) break;
            Evaluate(line);
        }
    }
}