using System.Text;

namespace Tarok;

/// <summary>
/// Front end of the interpreter that reports the error. 
/// </summary>
/// <param name="Line"></param>
/// <param name="Column"></param>
/// <param name="Message"></param>
public sealed record TarotError(int Line, int Column, string Message)
{
    public int Line { get; } = Line;
    public int Column { get; } = Column;
    public string Message { get; } = Message;

    public string Report(string where)
    {
        string errorLine = $"{Line} | ";
        string firstRow =$"{errorLine}{where}: {Message}"; 
        StringBuilder sb = new();
        for (int j = 0; j < errorLine.Length; j++)
        {
            sb.Append(" ");
        }
        for (int i = 0; i < Column; i++)
        {
            sb.Append("-");
        }
        sb.Append("^");
        return $"{firstRow}\n{sb.ToString()}";
    }
}