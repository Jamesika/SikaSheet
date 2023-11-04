#if TOOLS

using System;
using Godot;

namespace SikaSheet;

[Tool]
public partial class SheetDataCell : SheetCell
{
    [Export] private Control _contentRoot;
    [Export] private ColorRect _disableColorRect;
    [Export] private Button _arrayAddButton;

    public Type ValuePropertyType { get; private set; }
    private SheetValueProperty _valueProperty;
    
    private SheetColumnHeader _columnHeader;
    private SheetData _rowData;

    public Control FirstFocusable => _valueProperty.FirstFocusable;
    public Control LastFocusable => _valueProperty.LastFocusable;
    
    public void Initialize(SheetValueProperty valueProperty)
    {
        _valueProperty = valueProperty;
        ValuePropertyType = valueProperty.GetType();
        
        _valueProperty.ValueChanged += HandleValueChanged;
        _valueProperty.LayoutChanged += HandleLayoutChanged;
        _contentRoot.AddChild(_valueProperty);
        _valueProperty.SetupFocus();
        
        _disableColorRect.Visible = false;
        _arrayAddButton.Visible = false;
        _arrayAddButton.Pressed += HandleClickArrayAddButton;
    }

    public void UpdateCell(SheetColumnHeader columnHeader, SheetData rowData)
    {
        _columnHeader = columnHeader;
        _rowData = rowData;

        var dataValue = _columnHeader.GetDataValue(_rowData, out var isOutOfArrayRange, out var enableAddArrayElement, out var isNestedDataNull);
        var addButtonVisible = false;
        if (isOutOfArrayRange || isNestedDataNull)
        {
            if (isOutOfArrayRange)
            {
                if (enableAddArrayElement)
                    addButtonVisible = true;
            }
            else
            {
                SheetLogger.LogError($"NestedDataNull : c{_columnHeader.HeaderIndex}, r{_rowData.Index}!");
            }

            _valueProperty.Visible = false;
            _disableColorRect.Visible = true;
        }
        else
        {  
            _valueProperty.Visible = true;
            _valueProperty.DataType = _columnHeader.ColumnDataType;
            _valueProperty.RawValue = dataValue;
            _disableColorRect.Visible = false;
        }

        _arrayAddButton.Visible = addButtonVisible;
        
        // highlight 
        IsHighlight = DataSheetEditor.Instance.SheetReader.HighlightRow == rowData.Index;
        IsError = !_columnHeader.Validate(rowData);
    }

    public void HandleRecycle()
    {
        if (FirstFocusable != null)
        {
            FirstFocusable.FocusNext = null;
            FirstFocusable.FocusPrevious = null;
            LastFocusable.FocusNext = null;
            LastFocusable.FocusPrevious = null;
        }
        Visible = false;
    }

    #region [Handle Events]
    
    private void HandleValueChanged(object value)
    {
        DataSheetEditor.Instance.SheetReader.SetDataValue(_columnHeader, _rowData, value);
    }

    private void HandleLayoutChanged()
    {
        DataSheetEditor.Instance.SheetView.SetDirty();
    }

    private void HandleClickArrayAddButton()
    {
        DataSheetEditor.Instance.SheetReader.AddArrayElement(_columnHeader, _rowData);
    }

    #endregion
}

#endif