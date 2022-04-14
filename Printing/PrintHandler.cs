namespace IA_AEstrela.Printing;

public class PrintHandler
{

    public Stack<ConsoleColor> PreviousBackgroundColors { get; } = new();
    public Stack<ConsoleColor> PreviousForegroundColors { get; } = new();

    public int MaxWidth { get; private set; }

    public string LineStart { get; private set; }
    public Stack<string> PreviousLineStart { get; } = new();
    public string LineEnd { get; private set; }
    public Stack<string> PreviousLineEnd { get; } = new();

    public int CurrentPos { get; private set; }

    public int CurrentMaxWidth => MaxWidth - LineEnd.Length;
    public int RemainingWidth => CurrentMaxWidth - CurrentPos;


    #region Initializers
    public PrintHandler(int minWidth = 120)
    {
        if(System.OperatingSystem.IsWindows() && Console.WindowWidth < minWidth)
            Console.WindowWidth = minWidth;

        MaxWidth = Console.WindowWidth;

        CurrentPos = 0;
        LineStart = string.Empty;
        LineEnd = string.Empty;
    }

    private static PrintHandler? _Singleton = null;
    public static PrintHandler Init(int minWidth = 140) => _Singleton ??= new PrintHandler(minWidth);

    #endregion

    public PrintHandler Print(string text, bool allowMultiline = false)
    {
        var remainingWidth = RemainingWidth;
        var textLength = text.Length;
        if(textLength > remainingWidth)
        {
            var croppedText = text[..remainingWidth];
            Console.Write(croppedText);
            CurrentPos += remainingWidth;

            if(!allowMultiline) return this;

            BreakLine();
            var remainingText = text[remainingWidth..];
            Print(remainingText, allowMultiline);

        }
        else
        {
            Console.Write(text);
            CurrentPos += textLength;
        }

        return this;
    }

    public PrintHandler BreakLine()
    {
        Console.Write(LineStart);
        CurrentPos = LineStart.Length;
        return this;
    }

    public PrintHandler PrintPattern(string pattern)
    {
        var patternSize = pattern.Length;
        var repetitionCount = (RemainingWidth / patternSize) + 1;
        var repeatedPatternText = string.Join(string.Empty,
                                              Enumerable.Range(0, repetitionCount)
                                                        .Select(x => pattern));
        var croppedPatternText = repeatedPatternText[..RemainingWidth];
        Print(croppedPatternText);
        return this;
    }

    #region Table
    public class ColumnDefinition
    {
        public double WidthFraction { get; set; }
    }

    private IEnumerable<ColumnDefinition>? _columnDefinitions = null;

    public PrintHandler StartTable(params ColumnDefinition[] columnDefinitions)
    {
        if (_columnDefinitions is not null)
            throw new Exception("Another table cannot be started before ending the last one");

        _columnDefinitions = columnDefinitions;

        return this;
    }


    #endregion

}
