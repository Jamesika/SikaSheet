#if TOOLS

using System.Collections.Generic;
using Godot;

namespace SikaSheet;

[Tool]
public partial class SheetIndexView : SheetSubView
{
    [Export] private Control _contentRoot;
    [Export] private Control _content;
    [Export] private PackedScene _sheetCellPackedScene;

    private List<SheetIndexCell> _cells = new();

    public override void _Ready()
    {
        base._Ready();
    }

    public override bool GetMinRowSize(int index, out float size)
    {
        size = default;
        return false;
    }

    public override bool GetMinColumnSize(int index, out float size)
    {
        size = default;
        return false;
    }

    public override void PreRebuild()
    {
    }

    public override void Rebuild()
    {
        var visibleRange = MainView.RowVisibleIndexRange;
        var visibleCount = visibleRange.Y >= visibleRange.X ? (visibleRange.Y - visibleRange.X + 1) : 0;
        while (_cells.Count > visibleCount)
        {
            var lastCell = _cells[^1];
            _cells.RemoveAt(_cells.Count - 1);
            RecycleCell(lastCell);
        }

        while (_cells.Count < visibleCount)
        {
            _cells.Add(GetCell());
        }

        var rowIndex = visibleRange.X;
        var width = _content.Size.X;
        for (var i = 0; i < _cells.Count; i++)
        {
            var cell = _cells[i];
            var height = MainView.RowsSize[rowIndex];
            var rowBeginPosition = MainView.RowBeginPosition[rowIndex];
            cell.Position = new Vector2(0, rowBeginPosition);
            cell.Size = new Vector2(width, height);
            cell.UpdateCell(rowIndex);
            rowIndex++;
        }

        _contentRoot.Position = new Vector2(0, -MainView.ScrollPixelValue.Y);
        _content.CustomMinimumSize = new Vector2(0, MainView.RowsTotalSize);
        //_content.Size = new Vector2(width, MainView.RowsTotalSize);
    }

    public override void ClearCache()
    {
    }
    
    #region [Pool]
    
    private Stack<SheetIndexCell> _cellPool = new();
    
    private SheetIndexCell GetCell()
    {
        SheetIndexCell cell = null;
        if (_cellPool.Count > 0)
        {
            cell = _cellPool.Pop();
            _content.AddChild(cell);
        }
        else
        {
            cell = _sheetCellPackedScene.Instantiate<SheetIndexCell>();
            _content.AddChild(cell);
        }

        return cell;
    }

    private void RecycleCell(SheetIndexCell dataCell)
    {
        _content.RemoveChild(dataCell);
        _cellPool.Push(dataCell);
    }
 
    #endregion
}

#endif 