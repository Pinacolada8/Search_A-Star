
using IA_AEstrela.Utils;

namespace IA_AEstrela.Printing;

public static partial class PrintHandlerExtensions
{
    public static TablePrinter StartTable(this PrintHandler printHandler,
                                          TBDefinition tbDefinition)
    {
        var tablePrinter = new TablePrinter(tbDefinition, printHandler);


        return tablePrinter;
    }
}

#region POCOs
public class TBColumnDefinition
{
    public double WidthFraction { get; set; } = 1;

    internal int CalculatedWidth { get; set; } = 0;

    public TextAlignment ColumnAlignment { get; set; } = TextAlignment.CENTER;
    public ConsoleColor ColumnColor { get; set; } = Console.ForegroundColor;

    public string Header { get; set; } = string.Empty;
    public TextAlignment HeaderAlignment { get; set; } = TextAlignment.CENTER;
    public ConsoleColor? HeaderColor { get; set; }
}

public class TBDefinition
{
    public bool PrintHeaders { get; set; } = false;
    public ConsoleColor HeaderColor { get; set; } = Console.ForegroundColor;

    public IEnumerable<TBColumnDefinition> ColumnDefinitions { get; init; } = new List<TBColumnDefinition>();

    public ConsoleColor TableBackgroundColor { get; set; } = Console.BackgroundColor;
    public ConsoleColor TableForegroundColor { get; set; } = Console.ForegroundColor;

    public string LateralBorders { get; set; } = "||";
    public string VerticalBorders { get; set; } = "=";
    public string ColumnSeparator { get; set; } = " | ";
    public string LineSeparator { get; set; } = "-";
}

public class TBCell
{
    public string Value { get; set; } = string.Empty;

    public TextAlignment? Alignment { get; set; }
    public ConsoleColor? Color { get; set; }

    public static implicit operator TBCell(string value) => new() { Value = value };
}

public class TBRow
{
    public TBCell?[] Cells { get; }

    public TBRow(int rowSize)
    {
        Cells = new TBCell[rowSize];
    }
}

#endregion

public class TablePrinter
{
    public PrintHandler PrintHandler { get; init; }

    private readonly TBDefinition _tableDefinition;

    public readonly List<TBRow> Rows = new();

    private int _currentColumn = 0;

    #region Utils Methods

    public int ColumnQty() => _tableDefinition.ColumnDefinitions.Count();

    public TBRow LastRow() => Rows.Last();

    #endregion

    public TablePrinter(TBDefinition tableDefinition, PrintHandler? printHandler = null)
    {
        if(!tableDefinition.ColumnDefinitions.Any())
            throw new ArgumentException("At least one column is necessary to build a table");

        PrintHandler ??= PrintHandler.Init();
        _tableDefinition = tableDefinition;
    }

    #region Table Building
    public TablePrinter AddCell(TBCell cell, int? column = null, int? row = null)
    {
        var rowObj = row.HasValue ? Rows[row.Value] : LastRow();

        if(!column.HasValue)
        {
            column = _currentColumn;
            _currentColumn++;
        }

        rowObj.Cells[column.Value] = cell;

        return this;
    }

    public TablePrinter AddCells(params TBCell[] cells)
    {
        foreach(var cell in cells)
            AddCell(cell);

        return this;
    }

    public TablePrinter AddRow()
    {
        Rows.Add(new TBRow(ColumnQty()));
        _currentColumn = 0;
        return this;
    }

    public TablePrinter AddRow(params TBCell[] cells)
    {
        AddRow();
        AddCells(cells);

        return this;
    }

    #endregion

    #region Printing
    public PrintHandler PrintTable()
    {
        PrintHandler.BreakLine(ifNotNewLine: true)
                    .ChangeBackgroundColor(_tableDefinition.TableBackgroundColor)
                    .ChangeForegroundColor(_tableDefinition.TableForegroundColor);

        var extraLength = CalculateColumnWidths();

        PrintVerticalBorder();
        PrintHandler.AddLineStart(_tableDefinition.LateralBorders.PadLeft(extraLength / 2))
                    .AddLineEnd(_tableDefinition.LateralBorders.PadRight(extraLength / 2, ' '));

        if(_tableDefinition.PrintHeaders)
            PrintHeaders();

        var isFirstRow = true;
        foreach(var row in Rows)
        {
            if(!isFirstRow)
                PrintHandler.PrintPatternLine(_tableDefinition.LineSeparator);

            isFirstRow = false;

            PrintRow(row);
        }

        PrintHandler.RevertLineStart()
                    .RevertLineEnd();
        PrintVerticalBorder();
        PrintHandler.RevertForegroundColor()
                    .RevertBackgroundColor();

        return PrintHandler;
    }

    private int CalculateColumnWidths()
    {
        var columns = _tableDefinition.ColumnDefinitions.AsList()!;
        var columnQty = ColumnQty();

        var availableLength = PrintHandler.CurrentMaxLength -
                              ((columnQty - 1) * _tableDefinition.ColumnSeparator.Length) -
                              (2 * _tableDefinition.LateralBorders.Length);

        var fractionsSum = columns.Sum(x => x.WidthFraction);
        var singleFractionLength = availableLength / fractionsSum;

        columns.ForEach(x => x.CalculatedWidth = Convert.ToInt32(Math.Floor(x.WidthFraction * singleFractionLength)));

        var extraLength = availableLength - columns.Sum(x => x.CalculatedWidth);

        return extraLength;
    }

    private void PrintVerticalBorder() => PrintHandler.BreakLine(ifNotNewLine: true)
                                                      .PrintPatternLine(_tableDefinition.VerticalBorders);

    private void PrintHeaders()
    {
        var firstColumn = true;
        foreach(var column in _tableDefinition.ColumnDefinitions)
        {
            if(!firstColumn)
                PrintHandler.Print(_tableDefinition.ColumnSeparator);

            firstColumn = false;

            PrintHandler.Print("")
                        .ChangeForegroundColor(column.HeaderColor ?? _tableDefinition.HeaderColor)
                        .Print(column.Header.ToFixedLength(column.CalculatedWidth, column.HeaderAlignment))
                        .RevertForegroundColor();
        }

        PrintVerticalBorder();
    }

    private void PrintRow(TBRow row)
    {
        var columns = _tableDefinition.ColumnDefinitions.AsList()!;
        var firstColumn = true;

        for(var i = 0; i < columns.Count(); i++)
        {
            if(!firstColumn)
                PrintHandler.Print(_tableDefinition.ColumnSeparator);

            firstColumn = false;

            var column = columns[i];
            var cell = row.Cells[i] ?? new TBCell();

            var color = cell.Color ?? column.ColumnColor;
            var alignment = cell.Alignment ?? column.ColumnAlignment;
            PrintHandler.Print("")
                        .ChangeForegroundColor(color)
                        .Print(cell.Value.ToFixedLength(column.CalculatedWidth, alignment))
                        .RevertForegroundColor();
        }

        PrintHandler.BreakLine();
    }
    #endregion

}
