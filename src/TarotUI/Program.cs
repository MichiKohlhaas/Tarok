using System.Text;

const char Corner = '+';
const char Horizontal = '-';
const char Vertical = '|';

const byte CellWidth = 7;
const byte CellHeight = 2;
const byte CellPadding = 6;

const byte CursorUpDown = 2;
const byte CursorLeftRight = 8;

int cursOrigRow;
int cursOrigCol;

int height;
int width;
if (args.Length > 2)
{
    (width, height) = (int.Parse(args[0]), int.Parse(args[1]));
}
else
{
    height = 7;
    width = 10;
}

// cases
//bool isFirstRow;
bool isLastRow;
//bool isFirstCol;
bool isLastCol;

void DrawGrid()
{
    Console.Clear();
    Console.SetCursorPosition(0,0);
    for (var row = 0; row < height; row++)
    {
        for (var col = 0; col < width; col++)
        {
            DrawCell(row, col, "");
        }
        
    }
}

void DrawCell(int gridRow, int gridCol, string content, bool highlighted = false)
{
    StringBuilder cellBuilder = new ();
    
    var cursorPosition = GridToTerminal(gridRow, gridCol);
    var newCursLeft = cursOrigCol + cursorPosition.termCol;
    var newCursTop = cursOrigRow + cursorPosition.termRow;
    Console.SetCursorPosition(newCursLeft, newCursTop);
    
    // top border
    cellBuilder.Append(Corner);
    cellBuilder.Append(Horizontal, CellPadding);
    cellBuilder.Append(Corner);
    Console.Write(cellBuilder.ToString());
    cellBuilder.Clear();
    
    // content row
    cellBuilder.Append(Vertical);
    Console.SetCursorPosition(newCursLeft, newCursTop + 1);
    Console.Write(cellBuilder.ToString());
    cellBuilder.Clear();
    if (highlighted)
    {
        //Console.SetCursorPosition(newCursLeft, newCursTop + 1);
        Console.BackgroundColor = ConsoleColor.DarkCyan;
        if (string.IsNullOrWhiteSpace(content)) cellBuilder.Append(' ', CellPadding);
        else cellBuilder.Append(content);
        Console.Write(cellBuilder.ToString());
        Console.ResetColor();
        cellBuilder.Clear();
    }
    else
    {
        if (string.IsNullOrWhiteSpace(content)) cellBuilder.Append(' ', CellPadding);
        else cellBuilder.Append(content);
    }
    cellBuilder.Append(Vertical);
    Console.Write(cellBuilder.ToString());
    cellBuilder.Clear();
    
    // bottom row
    cellBuilder.Append(Corner);
    cellBuilder.Append(Horizontal, CellPadding);
    cellBuilder.Append(Corner);
    Console.SetCursorPosition(newCursLeft, newCursTop + 2);
    Console.Write(cellBuilder.ToString());
}

(int termRow, int termCol) GridToTerminal(int gridRow, int gridCol)
{
    var termRow = Math.Clamp(gridRow * CellHeight, 0, int.MaxValue);
    var termCol = Math.Clamp(gridCol * CellWidth, 0, int.MaxValue);

    return (termRow, termCol);
}


Console.SetCursorPosition(0, 0);
cursOrigCol = Console.CursorLeft;
cursOrigRow = Console.CursorTop;
Console.CursorVisible = false;
int cursorRow = 0;
int cursorCol = 0;

try
{
    DrawGrid();
    DrawCell(0,0, "", true);
    Console.CursorTop++;
    Console.CursorLeft = 0;
    
    bool quit = false;
    
    Console.SetCursorPosition(1,1);
    while (!quit)
    {
        var key = Console.ReadKey(intercept: true);
        switch (key.Key)
        {
            case ConsoleKey.Q:
                quit = true;
                break;
            case ConsoleKey.LeftArrow:
                // Redraw cell to normal
                DrawCell(cursorRow, cursorCol, "");
                // Draw cell to right highlighted
                cursorCol = Math.Clamp(cursorCol - 1, 0, width - 1);
                DrawCell(cursorRow, cursorCol, "", true);
                break;
            case ConsoleKey.RightArrow:
                // Redraw cell to normal
                DrawCell(cursorRow, cursorCol, "");
                // Draw cell to right highlighted
                cursorCol = Math.Clamp(cursorCol + 1, 0, width - 1);
                DrawCell(cursorRow, cursorCol, "", true);
                break;
            case ConsoleKey.UpArrow:
                // Redraw cell to normal
                DrawCell(cursorRow, cursorCol, "");
                // Draw cell to right highlighted
                cursorRow = Math.Clamp(cursorRow - 1, 0, height - 1);
                DrawCell(cursorRow, cursorCol, "", true);
                break;
            case ConsoleKey.DownArrow:
                // Redraw cell to normal
                DrawCell(cursorRow, cursorCol, "");
                // Draw cell to right highlighted
                cursorRow = Math.Clamp(cursorRow + 1, 0, height - 1);
                DrawCell(cursorRow, cursorCol, "", true);
                break;
            default: break;
        }
    }
}
finally
{
    Console.CursorVisible = true;
    Console.ResetColor();
    Console.Clear();
}
