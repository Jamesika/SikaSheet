#if TOOLS

using System.Collections.Generic;
using System.Linq;
using Godot;

namespace SikaSheet;

[Tool]
public partial class SheetDataView : SheetSubView
{
    [Export] private ScrollContainer _scrollContainer;
    [Export] private Control _content;
    [Export] private PackedScene _sheetDataCellPackedScene;

    private float _dataViewExtraSize = 233;      
    private Stack<SheetDataCell> _pool = new();
    private Dictionary<Vector2I, SheetDataCell> _dataCells = new();
    
    private List<float> _rowsMinSize = new();
    private List<float> _columnsMinSize = new();

    private Dictionary<Vector2I, Vector2> _cacheSize = new();

    private SheetDataCellFactory _dataCellFactory;
    private SheetDataCellFactory DataCellFactory => _dataCellFactory ??= new SheetDataCellFactory(_content, _sheetDataCellPackedScene);
    
    public override void _Ready()
    {
        base._Ready();
        _scrollContainer.GetHScrollBar().ValueChanged += HandleScrollEvent;
        _scrollContainer.GetVScrollBar().ValueChanged += HandleScrollEvent;
    }

    private void HandleScrollEvent(double value)
    {
        MainView.SetDirty();
    }
 
    public override bool GetMinRowSize(int index, out float size)
    {
        size = _rowsMinSize[index];
        return true;
    }

    public override bool GetMinColumnSize(int index, out float size)
    {
        size = _columnsMinSize[index];
        return true;
    }

    public override void PreRebuild()
    {
        var reader = MainView.SheetReader;
        _columnsMinSize.Clear();
        _rowsMinSize.Clear();
        for (var i = 0; i < reader.Columns.Count; i++)
            _columnsMinSize.Add(default);
        for (var i = 0; i < reader.RowDataList.Count; i++)
            _rowsMinSize.Add(default);

        foreach (var kv in _dataCells)
        {
            var pos = kv.Key;
            var minSize = kv.Value.GetCombinedMinimumSize();
            _cacheSize[pos] = minSize;
        }

        foreach (var kv in _cacheSize)
        {
            var pos = kv.Key;
            var minSize = kv.Value;
            
            // pos out of range
            if(pos.X >= _columnsMinSize.Count || pos.Y >= _rowsMinSize.Count)
                continue;
            
            _columnsMinSize[pos.X] = Mathf.Max(_columnsMinSize[pos.X], minSize.X);
            _rowsMinSize[pos.Y] = Mathf.Max(_rowsMinSize[pos.Y], minSize.Y);
        }
    }

    public override void Rebuild()
    {
        var columnRange = MainView.ColumnVisibleIndexRange;
        var rowRange = MainView.RowVisibleIndexRange;

        // Content Size 
        _content.CustomMinimumSize = new Vector2(
            MainView.ColumnsTotalSize + _dataViewExtraSize,
            MainView.RowsTotalSize + _dataViewExtraSize);
        
        // Remove invisible
        var removeList = _dataCells
            .Keys
            .Where(pos => pos.X < columnRange.X || pos.X > columnRange.Y || pos.Y < rowRange.X || pos.Y > rowRange.Y)
            .ToList();
        foreach (var pos in removeList)
        {
            var cell = _dataCells[pos];
            _dataCells.Remove(pos);
            DataCellFactory.Recycle(cell);
        }

        // Add Visible
        var sheetReader = MainView.SheetReader;
        for (int row = rowRange.X; row <= rowRange.Y; row++)
        for (int column = columnRange.X; column <= columnRange.Y; column++)
        {
            var pos = new Vector2I(column, row);
            var header = sheetReader.Columns[column];
            
            if (!_dataCells.TryGetValue(pos, out var cell))
            {
                // create cell
                cell = DataCellFactory.Create(header);
                _dataCells.Add(pos, cell);
            }
            else if (!DataCellFactory.IsMatch(header, cell))
            {
                // replace cell
                DataCellFactory.Recycle(cell);
                cell = DataCellFactory.Create(header);
                _dataCells[pos] = cell;
            }
            
            // update cell
            var cellBeginPosX = MainView.ColumnBeginPosition[pos.X];
            var bellBeginPosY = MainView.RowBeginPosition[pos.Y];
            var cellSizeX = MainView.ColumnsSize[pos.X];
            var cellSizeY = MainView.RowsSize[pos.Y];
            cell.Position = new Vector2(cellBeginPosX, bellBeginPosY);
            cell.Size = new Vector2(cellSizeX, cellSizeY);
            cell.UpdateCell(header, sheetReader.RowDataList[row]);
        }
        
        // Focus Prev/Next
        Control previousFocusNode = null;
        for (int row = rowRange.X; row <= rowRange.Y; row++)
        for (int column = columnRange.X; column <= columnRange.Y; column++)
        {
            var pos = new Vector2I(column, row);
            if (!_dataCells.TryGetValue(pos, out var cell))
                continue;

            if (cell.FirstFocusable != null && cell.FirstFocusable.IsVisibleInTree())
            {
                if (previousFocusNode != null)
                {
                    previousFocusNode.FocusNext = cell.FirstFocusable.GetPath();
                    cell.FirstFocusable.FocusPrevious = previousFocusNode.GetPath();
                }

                previousFocusNode = cell.LastFocusable;
            }
        }
    }

    public override void ClearCache()
    {
        _cacheSize.Clear();
    }

    public Vector2I GetScrollPixelValue()
    {
        return new Vector2I(_scrollContainer.ScrollHorizontal, _scrollContainer.ScrollVertical);
    }
}

#endif 