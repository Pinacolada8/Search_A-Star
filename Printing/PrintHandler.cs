using IA_AEstrela.Utils;

namespace IA_AEstrela.Printing;

public class PrintHandler
{
    public ConsoleColor CurrentBackgroundColor { get; private set; } = Console.BackgroundColor;
    private readonly Stack<ConsoleColor> _previousBackgroundColor = new();
    public ConsoleColor CurrentForegroundColor { get; private set; } = Console.ForegroundColor;
    private readonly Stack<ConsoleColor> _previousForegroundColor = new();

    public string CurrentLineStart { get; private set; } = string.Empty;
    private readonly Stack<string> _previousLineStart = new();
    public string CurrentLineEnd { get; private set; } = string.Empty;
    private readonly Stack<string> _previousLineEnd = new();

    public int MaxWidth => Console.WindowWidth;

    public int CurrentPos { get; private set; } = 0;

    public int CurrentLength => (CurrentPos - CurrentLineStart.Length).ToPositive();
    public int CurrentMaxLength => MaxWidth - CurrentLineStart.Length - CurrentLineEnd.Length;
    public int RemainingLength => (CurrentMaxLength - CurrentLength).ToPositive();
    public bool IsNewLine => CurrentPos == 0;

    #region Initializers
    public PrintHandler(int minWidth = 120)
    {
        if(System.OperatingSystem.IsWindows() && Console.WindowWidth < minWidth)
            Console.WindowWidth = minWidth;
    }

    private static PrintHandler? _Singleton = null;
    public static PrintHandler Init(int minWidth = 140) => _Singleton ??= new PrintHandler(minWidth);

    #endregion

    #region Printing
    private void PrintToConsole(string text)
    {
        if(CurrentPos > (MaxWidth - CurrentLineEnd.Length))
            return;

        var previousBackgroundColor = Console.BackgroundColor;
        var previousForegroundColor = Console.ForegroundColor;
        Console.BackgroundColor = CurrentBackgroundColor;
        Console.ForegroundColor = CurrentForegroundColor;
        if(IsNewLine)
        {
            Console.Write(CurrentLineStart);
            CurrentPos += CurrentLineStart.Length;
        }

        Console.Write(text);
        CurrentPos += text.Length;

        if(RemainingLength <= 0)
        {
            Console.Write(CurrentLineEnd);
            CurrentPos += CurrentLineStart.Length;
        }
        Console.BackgroundColor = previousBackgroundColor;
        Console.ForegroundColor = previousForegroundColor;
    }

    public PrintHandler BreakLine(bool ifNotNewLine = false)
    {
        if(ifNotNewLine && IsNewLine)
            return this;

        var padding = string.Empty.PadRight(RemainingLength);
        Print(padding);
        Console.WriteLine();
        CurrentPos = 0;

        return this;
    }

    public PrintHandler Print(string text, bool allowMultiline = false)
    {
        if(text.Length > RemainingLength)
        {
            var croppedText = text[..RemainingLength];
            var remainingText = text[RemainingLength..];
            PrintToConsole(croppedText);

            if(!allowMultiline) return this;

            BreakLine();
            Print(remainingText, allowMultiline);
        }
        else
        {
            PrintToConsole(text);
        }

        return this;
    }

    public PrintHandler PrintLine(string text, bool allowMultiline = false) 
        => Print(text, allowMultiline).BreakLine();

    public PrintHandler PrintPattern(string pattern)
    {
        var patternSize = pattern.Length;
        var repetitionCount = RemainingLength / patternSize;
        var repeatedPatternText = string.Join(string.Empty,
                                              Enumerable.Range(0, repetitionCount)
                                                        .Select(x => pattern));
        var croppedPatternText = repeatedPatternText[..RemainingLength];
        Print(croppedPatternText);
        return this;
    }

    public PrintHandler PrintPatternLine(string pattern)
        => PrintPattern(pattern).BreakLine();
    #endregion

    #region LineModifiers
    public PrintHandler AddLineStart(string text)
    {
        _previousLineStart.Push(CurrentLineStart);
        CurrentLineStart = $"{CurrentLineStart}{text}";
        return this;
    }

    public PrintHandler RevertLineStart()
    {
        CurrentLineStart = _previousLineStart.TryPop(out var lineStart) ? lineStart : string.Empty;
        return this;
    }

    public PrintHandler AddLineEnd(string text)
    {
        _previousLineEnd.Push(CurrentLineEnd);
        CurrentLineEnd = $"{text}{CurrentLineEnd}";
        return this;
    }

    public PrintHandler RevertLineEnd()
    {
        CurrentLineEnd = _previousLineEnd.TryPop(out var lineStart) ? lineStart : string.Empty;
        return this;
    }
    #endregion

    #region Color Modifiers

    public PrintHandler ChangeBackgroundColor(ConsoleColor color)
    {
        _previousBackgroundColor.Push(color);
        CurrentBackgroundColor = color;
        return this;
    }

    public PrintHandler RevertBackgroundColor()
    {
        CurrentBackgroundColor = _previousBackgroundColor.TryPop(out var res)
                                     ? res
                                     : Console.BackgroundColor;
        return this;
    }

    public PrintHandler ChangeForegroundColor(ConsoleColor color)
    {
        _previousForegroundColor.Push(color);
        CurrentForegroundColor = color;
        return this;
    }

    public PrintHandler RevertForegroundColor()
    {
        CurrentForegroundColor = _previousForegroundColor.TryPop(out var res)
                                     ? res
                                     : Console.ForegroundColor;
        return this;
    }

    #endregion
}
