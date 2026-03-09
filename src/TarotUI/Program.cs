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

void DrawCell(int gridRow, int gridCol, string content, bool highlighted = false, bool edit = false)
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
        Console.BackgroundColor = edit ? ConsoleColor.Gray : ConsoleColor.White;
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


// try set window first
try
{
    Console.SetWindowSize(120, 40);
    Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
}
catch
{
    // ignored
}

Console.SetCursorPosition(0, 0);
cursOrigCol = Console.CursorLeft;
cursOrigRow = Console.CursorTop;
Console.CursorVisible = true;
var cursorRow = 0;
var cursorCol = 0;

try
{
    DrawGrid();
    // Print text at bottom
    Console.SetCursorPosition(0, height * 2 + 1);
    Console.WriteLine("Q to quit");
    
    // Draw the cursor cell
    DrawCell(0,0, "", true);
    // Resetting the cursor
    Console.CursorTop = 1;
    Console.CursorLeft = 1;

    var state = State.Navigate;    
    var quit = false;
    string editBuffer = string.Empty;
    
    while (!quit)
    {
        
        var key = Console.ReadKey(intercept: true);
        
        
        switch (key.Key)
        {
            case ConsoleKey.Q:
                if (state == State.Edit) break;
                quit = true;
                break;
            case ConsoleKey.LeftArrow:
                if (state == State.Edit) break;
                MoveCursor(ref cursorCol, width, -1);
                break;
            case ConsoleKey.RightArrow:
                if (state == State.Edit) break;
                MoveCursor(ref cursorCol, width, 1);
                break;
            case ConsoleKey.UpArrow:
                if (state == State.Edit) break;
                MoveCursor(ref cursorRow, height, -1);
                break;
            case ConsoleKey.DownArrow:
                if (state == State.Edit) break;
                MoveCursor(ref cursorRow, height, 1);
                break;
            case ConsoleKey.Enter:
                state ^= State.Edit;
                if (state == State.Edit) SetupEdit();
                DrawCell(cursorRow, cursorCol, "", true, state == State.Edit);
                break;
            case ConsoleKey.Escape:
                if (state == State.Navigate) break;
                editBuffer = string.Empty;
                break;
            default:
                if (state == State.Edit)
                {
                    if (editBuffer.Length <= 5)
                    {
                        editBuffer += key.KeyChar;
                    }
                }
                break;
        }
    }
}
finally
{
    Console.CursorVisible = true;
    Console.ResetColor();
    Console.Clear();
}

void MoveCursor(ref int cursorPos, int dimension, int operation)
{
    // Redraw cell to normal
    DrawCell(cursorRow, cursorCol, "");
    // Draw cell to right highlighted
    cursorPos = Math.Clamp(cursorPos + operation, 0, dimension - 1);
    DrawCell(cursorRow, cursorCol, "", true);
}

void SetupEdit()
{
    Console.CursorVisible = true;
    var terminalPos = GridToTerminal(cursorRow, cursorCol);
    Console.SetCursorPosition(terminalPos.termRow, terminalPos.termCol);
}

[Flags]
internal enum State
{
    Navigate,
    Edit,
}