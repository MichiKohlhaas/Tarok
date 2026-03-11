using System.Diagnostics;
using System.Text;
using Tarok;

namespace TarotUI;

internal static class Program
{
    private static string[,]? _cellContents;
    private const char Corner = '+';
    private const char Horizontal = '-';
    private const char Vertical = '|';

    private const string ErrorMessage = "Invalid card, {0}, at row|col: {1}|{2}";

    private const byte CellWidth = 7;
    private const byte CellHeight = 2;
    private const byte CellPadding = 6;
    
    public static async Task Main(string[] args)
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
        _cellContents = new string[height, width];
        
        Lexer lexer = new Lexer();
        
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
                Console.BackgroundColor = edit ? ConsoleColor.Black : ConsoleColor.DarkCyan;
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
            var bottomPrintPos = height * 2 + 1;
            Console.SetCursorPosition(0, bottomPrintPos);
            Console.WriteLine("Q to quit\tEnter to edit, again to commit.\tEsc to cancel edit.");
    
            // Draw the cursor cell
            DrawCell(0,0, "", true);
            // Resetting the cursor
            Console.CursorTop = 1;
            Console.CursorLeft = 1;

            var state = State.Navigate;    
            var quit = false;
            var editBufferChars = new char[6];
            var counter = 0;
            var localCursorMin = 0;
            var localCursorMax = 0;
            var tempCellContents = "";
            
            while (!quit)
            {

                var key = Console.ReadKey(intercept: true);
                

                switch (key.Key)
                {
                    case ConsoleKey.Q:
                        if (state == State.Edit) continue;
                        quit = true;
                        break;
                    case ConsoleKey.LeftArrow:
                        if (state == State.Edit)
                        {
                            if (Console.CursorLeft - 1 < localCursorMin) break;
                            
                            Console.CursorLeft--;
                            counter = Math.Clamp(counter - 1, 0, 5);
                            break;
                        }
                        MoveCursor(ref cursorCol, width, -1);
                        break;
                    case ConsoleKey.RightArrow:
                        if (state == State.Edit)
                        {
                            if (Console.CursorLeft + 1 >= localCursorMax) break;

                            Console.CursorLeft++;
                            counter = Math.Clamp(counter + 1, 0, 5);
                            break;
                        }
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
                        
                        counter = 0;
                        var isEmptyCell = string.IsNullOrEmpty(_cellContents[cursorRow, cursorCol]);
                        if (state != State.Edit)
                        {
                            Array.Clear(editBufferChars, 0, editBufferChars.Length);
                            var text = _cellContents[cursorRow, cursorCol];
                            if (!isEmptyCell && !lexer.IsValidToken(text))
                            {
                                var tempCursorTopPos = Console.CursorTop;
                                var tempCursorLeftPos = Console.CursorLeft;
                                Console.SetCursorPosition(0, bottomPrintPos + 1);
                                var msg = string.Format(ErrorMessage, text, cursorRow, cursorCol);
                                // pad the right to clear text from last write
                                int padLength = (width * CellWidth) - msg.Length;
                                Console.WriteLine(msg.PadRight(padLength));
                                Console.SetCursorPosition(tempCursorLeftPos, tempCursorTopPos);
                            }
                            if (text.Length != CellPadding && !isEmptyCell)
                            {
                                _cellContents[cursorRow, cursorCol] = text.PadRight(CellPadding);
                            }
                        } 
                        else if (!isEmptyCell)
                        {
                            // entering edit mode, preserve the current cell's content in case esc is pressed
                            tempCellContents = _cellContents[cursorRow, cursorCol];
                            
                            // load the buffer with the last text if not empty
                            editBufferChars = _cellContents[cursorRow, cursorCol].ToCharArray();
                        }
                        
                        Console.CursorVisible = state == State.Edit;
                        DrawCell(cursorRow, cursorCol, _cellContents[cursorRow, cursorCol], true, state == State.Edit);
                        localCursorMin = Console.CursorLeft;
                        localCursorMax = localCursorMin + CellPadding;
                        break;
                    case ConsoleKey.Escape:
                        // stop editing, empty buffer, return cell's contents
                        if (state == State.Edit)
                        {
                            state = State.Navigate;
                            Array.Clear(editBufferChars, 0, editBufferChars.Length);
                            _cellContents[cursorRow, cursorCol] = tempCellContents;
                            DrawCell(cursorRow, cursorCol, _cellContents[cursorRow, cursorCol], true);
                        }
                        break;
                    default:
                        if (state == State.Edit)
                        {
                            
                            if (!char.IsControl(key.KeyChar))
                            {
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = ConsoleColor.White;
                                
                                
                                editBufferChars[counter] = key.KeyChar;
                                Console.Write(editBufferChars[counter]);
                                counter = Math.Clamp(counter + 1, 0, 5);
                                // if the counter is at the end of the buffer, don't advance the cursor
                                if (Console.CursorLeft >= localCursorMax)
                                {
                                    Console.CursorLeft--;
                                }
                                //Console.ResetColor();
                            }
                            else if (key.Key == ConsoleKey.Backspace)
                            {
                                Console.BackgroundColor = ConsoleColor.Black;
                                editBufferChars[counter] = ' ';
                                Console.Write(' ');
                                // undo the advance caused by Write
                                Console.CursorLeft--;
                                // move to previous spot if available
                                if (Console.CursorLeft - 1 < localCursorMin) Console.CursorLeft = localCursorMin;
                                else Console.CursorLeft--;
                                
                                counter = Math.Clamp(counter - 1, 0, 5);
                                
                                
                            }
                            Console.ResetColor();
                            _cellContents[cursorRow, cursorCol] = new string(editBufferChars).TrimStart('\0').TrimEnd('\0');
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
            DrawCell(cursorRow, cursorCol, _cellContents![cursorRow, cursorCol]);
            // Draw cell to right highlighted
            cursorPos = Math.Clamp(cursorPos + operation, 0, dimension - 1);
            DrawCell(cursorRow, cursorCol, _cellContents[cursorRow, cursorCol], true);
        }
    }
}

[Flags]
internal enum State
{
    Navigate,
    Edit,
}