class ReplEngine()
{
    public void Evaluate(string line)
    {
                   
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

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: tarot <script>");
        }
        else if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            ReplEngine engine = new ReplEngine();
            engine.Start();
        }
    }
    
    static void RunFile(string filePath)
    {
        
    }
}

