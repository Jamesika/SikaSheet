#if TOOLS

using System.Collections.Generic;
using Godot;

namespace SikaSheet;

[Tool]
public partial class SheetHeaderView : SheetSubView
{
    [Export] private Control _content;
    [Export] private PackedScene _headerCellPackedScene;

    private List<SheetColumnHeaderCell> _cells = new();

    public override bool GetMinRowSize(int index, out float size)
    {
        size = default;
        return false;
    }

    public override bool GetMinColumnSize(int index, out float size)
    {
        if (_cells.Count <= index)
        {
            size = default;
            return false;
        }

        var minSize = _cells[index].GetCombinedMinimumSize();
        size = minSize.X;
        return true;
    }

    public override void PreRebuild()
    {
        var columns = MainView.SheetReader.Columns;
        while (columns.Count > _cells.Count)
        {
            var headerCell = _headerCellPackedScene.Instantiate<SheetColumnHeaderCell>();
            _content.AddChild(headerCell);
            _cells.Add(headerCell);
        }

        while (columns.Count < _cells.Count)
        {
            var lastCell = _cells[^1];
            _cells.RemoveAt(_cells.Count - 1);
            lastCell.QueueFree();
        }
    }

    public override void Rebuild()
    {
        var columns = MainView.SheetReader.Columns;
        var height = _content.Size.Y;
        for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
        {
            var cell = _cells[columnIndex];
            var width = MainView.ColumnsSize[columnIndex];
            var columnBeginPosition = MainView.ColumnBeginPosition[columnIndex];
            cell.Position = new Vector2(columnBeginPosition, 0);
            cell.Size = new Vector2(width, height);
            cell.UpdateCell(columns[columnIndex]);
        }

        _content.Position = new Vector2(-MainView.ScrollPixelValue.X, 0);
        _content.CustomMinimumSize = new Vector2(MainView.ColumnsTotalSize, 0);
    }

    public override void ClearCache()
    {
    }
}

#endif 