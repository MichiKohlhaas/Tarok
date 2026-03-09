using System.Text;

namespace TarotUI;

internal static class Program
{
    private static string[,] cellContents;
    private const char Corner = '+';
    private const char Horizontal = '-';
    private const char Vertical = '|';

    private const byte CellWidth = 7;
    private const byte CellHeight = 2;
    private const byte CellPadding = 6;
    
    public static void Main(string[] args)
    {
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
        cellContents = new string[height, width];
        
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
    
            // reset cursor to be at the content part
            Console.SetCursorPosition(newCursLeft + 1, newCursTop + 1);
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
        Console.CursorVisible = false;
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
            var editBuffer = string.Empty;
            var editBufferChars = new char[6];
            var counter = 0;
            var localCursorMin = 0;
            var localCursorMax = 0;
            
            while (!quit)
            {

                var key = Console.ReadKey(intercept: state == State.Edit);
                

                switch (key.Key)
                {
                    case ConsoleKey.Q:
                        if (state == State.Edit) continue;
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
                        Console.CursorVisible = state == State.Edit;
                        DrawCell(cursorRow, cursorCol, "", true, state == State.Edit);
                        localCursorMin = Console.CursorLeft;
                        localCursorMax = localCursorMin + CellPadding;
                        break;
                    case ConsoleKey.Escape:
                        if (state == State.Navigate) break;
                        //editBuffer = string.Empty;
                        break;
                    default:
                        if (state == State.Edit)
                        {
                            
                            if (!char.IsControl(key.KeyChar))
                            {
                                Console.BackgroundColor = ConsoleColor.Gray;
                                Console.ForegroundColor = ConsoleColor.White;
                                
                                
                                editBufferChars[counter] = key.KeyChar;
                                Console.Write(editBufferChars[counter]);
                                counter = Math.Clamp(counter + 1, 0, 5);
                                // if the counter is at 4, don't advance the cursor
                                if (Console.CursorLeft >= localCursorMax)
                                {
                                    Console.CursorLeft--;
                                }
                                //Console.ResetColor();
                            }
                            else if (key.Key == ConsoleKey.Backspace)
                            {
                                Console.BackgroundColor = ConsoleColor.Gray;
                                editBufferChars[counter] = ' ';
                                Console.Write(' ');
                                // undo the advance caused by Write
                                Console.CursorLeft--;
                                // move to previous spot if available
                                if (Console.CursorLeft - 1 < localCursorMin) Console.CursorLeft = localCursorMin;
                                else Console.CursorLeft--;
                                
                                counter = Math.Clamp(counter - 1, 0, 4);
                            }
                            Console.ResetColor();
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
    }
}

[Flags]
internal enum State
{
    Navigate,
    Edit,
}