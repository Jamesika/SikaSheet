#if TOOLS

using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace SikaSheet;

[Tool]
public partial class SheetView : Control
{
    public EditorSheetReader SheetReader { get; private set; }
    private bool _dirtyFlag;
 
    #region [Row & Column Size]

    public List<float> RowsSize  { get; private set; } = new();
    public List<float> ColumnsSize { get; private set; } = new();

    public List<float> RowBeginPosition { get; private set; } = new();
    public List<float> ColumnBeginPosition { get; private set; } = new();
    
    public float RowsTotalSize { get; private set; }
    public float ColumnsTotalSize { get; private set; }

    private float _minColumnSize = 60;
    private float _minRowSize = 20;
    private float _visibleBias = 10;

    #endregion

    #region [Scroll Visibility]

    public Vector2 DataViewSize { get; private set; }
    public Vector2I ScrollPixelValue { get; private set; }
    public Vector2I ColumnVisibleIndexRange { get; private set; }
    public Vector2I RowVisibleIndexRange { get; private set; }

    #endregion

    #region [SubView]

    [Export] private SheetDataView _dataView;
    [Export] private SheetHeaderView _headerView;
    [Export] private SheetIndexView _indexView;
    private List<SheetSubView> _subViews = new List<SheetSubView>();
    
    #endregion
    
    public void Show(EditorSheetReader editorSheetReader)
    {
        if (SheetReader != null)
            ClearCache();

        SheetReader = editorSheetReader;
        SetDirty();
    }

    public override void _Ready()
    {
        base._Ready();
        _subViews.Add(_dataView);
        _subViews.Add(_headerView);
        _subViews.Add(_indexView);
        foreach (var sheetSubView in _subViews)
            sheetSubView?.Initialize(this);

        this.Resized += SetDirty;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_dirtyFlag)
        {
            var sw = Stopwatch.StartNew();
            
            _dirtyFlag = false;
            Rebuild();
            // maybe build twice in one frame!
            if(_dirtyFlag)
                Rebuild();
            
            // make sure proper layout
            Rebuild();
            
            SheetLogger.LogDebug($"Rebuild SheetView : {sw.ElapsedMilliseconds}ms");
        }
    }

    #region [Rebuild]

    public void ClearCache()
    {
        foreach (var sheetSubView in _subViews)
            sheetSubView.ClearCache();
    }

    public void SetDirty()
    {
        _dirtyFlag = true;
    }

    private void Rebuild()
    {
        if (!IsVisibleInTree() || SheetReader == null)
            return;
        
        PreRebuildSubViews();
        CalculateSize();
        CalculateVisibility();
        RebuildSubViews();
    }

    private void CalculateSize()
    {
        var columns = SheetReader.Columns;
        var rows = SheetReader.RowDataList;
        RowsTotalSize = 0f;
        ColumnsTotalSize = 0f;

        ColumnsSize.Clear();
        RowsSize.Clear();
        ColumnBeginPosition.Clear();
        RowBeginPosition.Clear();
        
        // set columns size 
        for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
        {
            var columnSize = _minColumnSize;
            foreach (var sheetSubView in _subViews)
            {
                if(sheetSubView.GetMinColumnSize(columnIndex, out var minColumnSize))
                    columnSize = Mathf.Max(columnSize, minColumnSize);
            }

            ColumnBeginPosition.Add(ColumnsTotalSize);
            ColumnsSize.Add(columnSize);
            ColumnsTotalSize += columnSize;
        }
        // set rows size 
        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var rowSize = _minRowSize;
            foreach (var sheetSubView in _subViews)
            {
                if (sheetSubView.GetMinRowSize(rowIndex, out var minRowSize))
                    rowSize = Mathf.Max(rowSize, minRowSize);
            }

            RowBeginPosition.Add(RowsTotalSize);
            RowsSize.Add(rowSize);
            RowsTotalSize += rowSize;
        }
    }
 
    private void CalculateVisibility()
    {
        DataViewSize = _dataView.Size;
        ScrollPixelValue = _dataView.GetScrollPixelValue();

        var columnVisibleBeginPosition = ScrollPixelValue.X;
        var columnVisibleEndPosition = ScrollPixelValue.X + DataViewSize.X;
        
        var rowVisibleBeginPosition = ScrollPixelValue.Y;
        var rowVisibleEndPosition = ScrollPixelValue.Y + DataViewSize.Y;

        Vector2I visibleRange = new Vector2I(int.MaxValue, int.MinValue);
        for (var i = 0; i < ColumnBeginPosition.Count; i++)
        {
            var beginPosition = ColumnBeginPosition[i];
            var size = ColumnsSize[i];
            if (beginPosition + size > columnVisibleBeginPosition - _visibleBias)
                visibleRange.X = Mathf.Min(visibleRange.X, i);

            if (beginPosition < columnVisibleEndPosition + _visibleBias)
                visibleRange.Y = Mathf.Max(visibleRange.Y, i);
        }
        ColumnVisibleIndexRange = visibleRange;

        visibleRange = new Vector2I(int.MaxValue, int.MinValue);
        for (var i = 0; i < RowBeginPosition.Count; i++)
        {
            var beginPosition = RowBeginPosition[i];
            var size = RowsSize[i];
            if (beginPosition + size > rowVisibleBeginPosition - _visibleBias)
                visibleRange.X = Mathf.Min(visibleRange.X, i);

            if (beginPosition < rowVisibleEndPosition + _visibleBias)
                visibleRange.Y = Mathf.Max(visibleRange.Y, i);
        }
        RowVisibleIndexRange = visibleRange;
    }

    private void PreRebuildSubViews()
    {
        foreach (var sheetSubView in _subViews)
            sheetSubView.PreRebuild();
    }

    private void RebuildSubViews()
    {
        foreach (var sheetSubView in _subViews)
            sheetSubView.Rebuild();
    }

    #endregion
}

#endif 