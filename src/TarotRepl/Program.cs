using Tarok;

namespace TarotRepl;

public class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: tarot <script>");
        }
        else if (args.Length == 1)
        {
            await RunFile(args[0]);
        }
        else
        {
            var engine = new ReplEngine();
            engine.Start();
        }
    }
    
    static async Task RunFile(string filePath)
    {
        var lexer = new Lexer();
        await lexer.LoadScript(filePath);
        if (lexer.Errors.Count > 0)
        {
            foreach (var error in lexer.Errors)
            {
                Console.WriteLine(error.Message);
            }
            return;
        }
        var tokens = lexer.ScanTokens();
    }
}

