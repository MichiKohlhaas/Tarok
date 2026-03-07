/*
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
*/

using System.Reflection.Metadata.Ecma335;
using System.Text;

const string TopLeft     = "┌";
const string TopRight    = "┐";
const string BottomLeft  = "└";
const string BottomRight = "┘";
const string Horizontal  = "──────";
const string Vertical    = "│";
const string TopJoin     = "┬";
const string BottomJoin  = "┴";
const string LeftJoin    = "├";
const string RightJoin   = "┤";
const string CenterJoin  = "┼";
    
//public string Input { get; set; } = string.Empty;

string FirstRowFirstCol = TopLeft;
string FirstRowMiddleCol = TopJoin;
string FirstRowLastCol = TopRight;
string MiddleRowFirstCol = LeftJoin;
string MiddleRowMiddleCol = CenterJoin;
string MiddleRowLastCol = RightJoin;
string LastRowFirstCol = BottomLeft;
string LastRowMiddleCol = BottomJoin;
string LastRowLastCol = BottomRight;
string HorizontalFill = Horizontal;
string ContentRow = Vertical;

int height;
int width;
if (args.Length > 2)
{
    (width, height) = (int.Parse(args[0]), int.Parse(args[1]));
}
else
{
    height = 25;
    width = 25;
}

State state = State.Navigation;
Grid grid = new(width, height);


void DrawGrid()
{
    StringBuilder topRowBuilder = new ();
    StringBuilder bottomRowBuilder = new ();
    StringBuilder contentRowBuilder = new();
    for (var i = 0; i <= height; i++) // Row
    {
        for (var j = 0; j <= width; j++) // Column
        {
            // FirstRowFirstCol
            if (i == 0 && j == 0)
            {
                topRowBuilder.Append(FirstRowFirstCol);
                topRowBuilder.Append(HorizontalFill);
                contentRowBuilder.Append(ContentRow);
                contentRowBuilder.Append(' ', 6);
                continue;
            }
            // FirstRowMiddleCol
            if (i == 0 && j != width)
            {
                topRowBuilder.Append(FirstRowMiddleCol);
                topRowBuilder.Append(HorizontalFill);
                
                contentRowBuilder.Append(ContentRow);
                contentRowBuilder.Append(' ', 6);
                continue;
            }
            // FirstRowLastCol
            if (i == 0 && j == width)
            {
                topRowBuilder.Append(FirstRowLastCol);
                contentRowBuilder.Append(ContentRow);
                continue;
            }
            // MiddleRowFirstCol
            if (i != 0 && i != height && j == 0)
            {
                topRowBuilder.Append(MiddleRowFirstCol);
                topRowBuilder.Append(HorizontalFill);
                contentRowBuilder.Append(ContentRow);
                contentRowBuilder.Append(' ', 6);
                continue;
            }
            // MiddleRowMiddleCol
            if (i != 0 && i != height && j != 0 && j != width)
            {
                topRowBuilder.Append(MiddleRowMiddleCol);
                topRowBuilder.Append(HorizontalFill);
                contentRowBuilder.Append(ContentRow);
                contentRowBuilder.Append(' ', 6);
                continue;
            }
            // MiddleRowLastCol
            if (i != 0 && i != height && j == width)
            {
                topRowBuilder.Append(MiddleRowLastCol);
                contentRowBuilder.Append(ContentRow);
                continue;
            }
            // LastRowFirstCol
            if (i == height && j == 0)
            {
                bottomRowBuilder.Append(LastRowFirstCol);
                bottomRowBuilder.Append(HorizontalFill);
                //contentRowBuilder.Append(ContentRow);
                //contentRowBuilder.Append(' ', 6);
                continue;
            }
            // LastRowMiddleCol
            if (i == height && j != 0 && j != width)
            {
                bottomRowBuilder.Append(LastRowMiddleCol);
                bottomRowBuilder.Append(HorizontalFill);
                //contentRowBuilder.Append(ContentRow);
                //contentRowBuilder.Append(' ', 6);
                continue;
            }
            // LastRowLastCol
            if (i == height && j == width)
            {
                bottomRowBuilder.Append(LastRowLastCol);
                //contentRowBuilder.Append(ContentRow);
            }
        }
        if (i != height)
        {
            topRowBuilder.AppendLine();
            topRowBuilder.Append(contentRowBuilder);
            topRowBuilder.AppendLine();
        }
        contentRowBuilder.Clear();
    }
    
    topRowBuilder.Append(bottomRowBuilder.ToString());
    Console.WriteLine(topRowBuilder.ToString());
}


void Input()
{
    while (true)
    {
        var keyInfo = Console.ReadKey(intercept: true);
        switch (state)
        {
            case State.Navigation: 
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    state = State.Edit;
                }
                else if (keyInfo.Key is 
                         ConsoleKey.LeftArrow or 
                         ConsoleKey.RightArrow or 
                         ConsoleKey.UpArrow or 
                         ConsoleKey.DownArrow)
                {
                    // calculate move cursor
                    // draw cursor in new position
                }
                break;
            case State.Edit:
                if (keyInfo.Key == ConsoleKey.Escape) state = State.Navigation;
                break;
            case State.Drawing:
                // Draw
                state = State.Navigation;
                break;
        }
    }
}

//Input();
DrawGrid();


enum State
{
    Navigation,
    Edit,
    Drawing,
}

class Grid(int width, int height)
{
    private const int CellWidth = 8;
    private const int CellHeight = 3;
    
    public int Width { get; set; } = width;
    public int Height { get; set; } = height;
    public Cell[,] Dimension { get; set; } = new Cell[width,height];

    private class Cursor
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
    private Cursor _cursor = new();
}

class Cell
{
    
}

class Renderer
{
    // terminal column = cell's column index * cell width
    public void RedrawGrid()
    {
        Console.Clear();
    }
    
    public void DrawGrid(){}

    public void DrawCursor()
    {
    }
    public void RedrawCell(){}
    
}