#if TOOLS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using Godot;

namespace SikaSheet;

/// <summary>
/// Read/Write sheet json data
/// </summary>
public class EditorSheetReader : SheetReader
{
    public event Action OnSheetDataChange;
    public event Action OnHistoryChange;
    
    public List<SheetColumnHeader> Columns = new List<SheetColumnHeader>();

    protected override bool EnableCacheJsonText => true;

    public EditorSheetReader(Type sheetDataType) : base(sheetDataType)
    {
    }
    
    private void HandleDataChange(bool maybeColumnsChange)
    {
        FixSheetDataIndex();
        
        if (maybeColumnsChange)
        {
            var newColumns = CreateSheetColumns();
            Columns.Clear();
            Columns.AddRange(newColumns);
        }

        TrySaveSheet();
        SheetDatabase.SetRuntimeSheetReaderDirty(SheetDataType);
        OnSheetDataChange?.Invoke();
    }

    /// <summary>
    /// Save sheet if any changes
    /// </summary>
    private void TrySaveSheet()
    {
        var sw = Stopwatch.StartNew();
        
        var sheetJson = SheetJsonUtility.ToJson(RowDataList, SheetDataType);
        if (sheetJson != CachedJsonText)
        {
            CachedJsonText = sheetJson;
            SheetDatabase.TryWriteSheetText(SheetDataType.Name, sheetJson);
            SheetLogger.LogDebug($"SaveSheet {SheetDataType.Name} ({sw.ElapsedMilliseconds}ms)");
        }
    }

    protected override void OnReadSheet()
    {
        Columns = CreateSheetColumns();
        FixSheetNullData();
        FixSheetResource();
        TryInitHistory();
    }
    
    #region [Editor View]

    public int HighlightRow { get; private set; } = -1;

    public void SetHighlightRow(int rowIndex)
    {
        HighlightRow = rowIndex;
        SheetLogger.LogDebug("Set highlight row : " + HighlightRow);
        HandleDataChange(false);
    }

    #endregion
    
    #region [Modify Sheet]

    private SheetData _copiedRow;

    public void SetDataValue(SheetColumnHeader header, SheetData rowData, object newValue)
    {
        if (header.SetDataValue(rowData, newValue))
        {
            HandleDataChange(false);
            TryAppendHistory($"SetDataValue at (column{header.HeaderIndex},row{rowData.Index})");
        }
    }

    public void AddArrayElement(SheetColumnHeader header, SheetData rowData)
    {
        header.AddArrayElement(rowData);
        HandleDataChange(true);
        TryAppendHistory($"AddArrayElement at (column{header.HeaderIndex},row{rowData.Index})");
    }
 
    public void AddRow()
    {
        var row = Activator.CreateInstance(SheetDataType) as SheetData;
        RowDataList.Add(row);
        FixSheetNullDataAtRow(RowDataList.Count - 1);
        HandleDataChange(false);
        TryAppendHistory($"AddRow");
    }

    public void DuplicateRow(int rowIndex)
    {
        if (RowDataList.Count <= rowIndex)
        {
            SheetLogger.LogError("DuplicateRow failed");
            return;
        }

        var data = RowDataList[rowIndex];
        var newData = CloneData(data);
        RowDataList.Insert(rowIndex + 1, newData);
        HandleDataChange(false);
        TryAppendHistory($"DuplicateRow {rowIndex}");
    }

    public void MoveRow(int rowIndex, int targetRowIndex)
    {
        if (RowDataList.Count <= rowIndex || RowDataList.Count < targetRowIndex)
        {
            SheetLogger.LogError("DuplicateRow failed");
            return;
        }

        if(rowIndex == targetRowIndex)
            return;

        var finalTargetRowIndex = targetRowIndex > rowIndex ? targetRowIndex - 1 : targetRowIndex;
        var data = RowDataList[rowIndex];
        RowDataList.RemoveAt(rowIndex);
        RowDataList.Insert(finalTargetRowIndex, data);
        
        HandleDataChange(false);
        TryAppendHistory($"MoveRow {rowIndex}->{targetRowIndex}");
    }

    public void DeleteRow(int rowIndex)
    {
        if (RowDataList.Count <= rowIndex)
        {
            SheetLogger.LogError("DeleteRow failed");
            return;
        }

        RowDataList.RemoveAt(rowIndex);
        HandleDataChange(true);
        TryAppendHistory($"DeleteRow {rowIndex}");
    }

    public bool HasCopiedRow()
    {
        return _copiedRow != null;
    }

    public void CopyRow(int rowIndex)
    {
        if (RowDataList.Count <= rowIndex)
        {
            SheetLogger.LogError("CopyRow failed");
            return;
        }

        _copiedRow = RowDataList[rowIndex];
        SheetLogger.Log($"CopyRow : {rowIndex}");
    }

    public void PasteRow(int rowIndex)
    {
        if (RowDataList.Count <= rowIndex || !HasCopiedRow())
        {
            SheetLogger.LogError("PasteRow failed");
            return;
        }

        var newRow = CloneData(_copiedRow);
        RowDataList[rowIndex] = newRow;
        HandleDataChange(true);
        TryAppendHistory($"PasteRow at {rowIndex}");
    }

    public void DeleteArrayIndex(SheetColumnHeader header)
    {
        for (var i = 0; i < RowDataList.Count; i++)
            header.DeleteArrayElement(RowDataList[i]);
        
        HandleDataChange(true);
        TryAppendHistory($"DeleteArrayIndex at {header.HeaderIndex}");
    }

    private SheetData CloneData(SheetData sourceData)
    {
        var json = SheetJsonUtility.ToJsonForSingleData(sourceData);
        return SheetJsonUtility.FromJsonForSingleData(json, sourceData.GetType());
    }

    private bool FixSheetNullData()
    {
        var flag = false;
        for (var rowIndex = 0; rowIndex < RowDataList.Count; rowIndex++)
        {
            if (FixSheetNullDataAtRow(rowIndex))
                flag = true;
        }

        if (flag)
        {
            SheetLogger.LogDebug("FixSheetNullData success");
            HandleDataChange(false);
        }

        return flag;
    }

    public bool FixSheetResource()
    {
        var flag = false;
        for (var rowIndex = 0; rowIndex < RowDataList.Count; rowIndex++)
        {
            if (FixSheetResourceAtRow(rowIndex))
                flag = true;
        }

        if (flag)
        {
            SheetLogger.LogDebug("FixSheetResource success");
            HandleDataChange(false);
        }

        return flag;
    }

    private bool FixSheetNullDataAtRow(int rowIndex)
    {
        var fixedFlag = false;
        for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
        {
            var columnHeader = Columns[columnIndex];
            var rowData = RowDataList[rowIndex];
            if (columnHeader.FixNullData(rowData))
                fixedFlag = true;
        }

        return fixedFlag;
    }

    private bool FixSheetResourceAtRow(int rowIndex)
    {
        var fixedFlag = false;
        for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
        {
            var columnHeader = Columns[columnIndex];
            var rowData = RowDataList[rowIndex];
            if (columnHeader.FixResource(rowData))
                fixedFlag = true;
        }

        return fixedFlag;
    }

    #endregion

    #region [Calculate Columns]
    
    private List<SheetColumnHeader> CreateSheetColumns()
    {
        var sheetFields = EditorSheetTypeUtility.GetSheetFields(SheetDataType);
        var columns = new List<SheetColumnHeader>();
        foreach (var sheetDataFieldInfo in sheetFields)
        {
            var fieldType = sheetDataFieldInfo.FieldType;
            var isNestedType = EditorSheetTypeUtility.IsNestedType(fieldType);
            var isArray = fieldType.IsArray;

            if(isNestedType)
            {
                AppendSheetColumnsByNestedData(fieldType, sheetDataFieldInfo, columns);
            }
            else if (isArray)
            {
                AppendSheetColumnsByNestedArray(fieldType, sheetDataFieldInfo, columns);
            }
            else
            {
                columns.Add(new SheetColumnHeader(this, sheetDataFieldInfo, false, default, false, default));
            }
        }

        SortColumns(columns);
        return columns;
    }

    private void SortColumns(List<SheetColumnHeader> columns)
    {
        for (var i = 0; i < columns.Count; i++)
            columns[i].HeaderIndex = i;

        columns.Sort((a, b) =>
        {
            var aValue = a.IsKey ? 1 : 0;
            var bValue = b.IsKey ? 1 : 0;
            if (aValue != bValue)
                return aValue - bValue;

            if (a.IsKey && b.IsKey)
            {
                aValue = -a.KeyIndex;
                bValue = -b.KeyIndex;
                return aValue - bValue;
            }

            aValue = a.BaseTypeDepth;
            bValue = b.BaseTypeDepth;
            if (aValue != bValue)
                return aValue - bValue;

            aValue = -a.HeaderIndex;
            bValue = -b.HeaderIndex;
            return aValue - bValue;
        });
        columns.Reverse();
        
        for (var i = 0; i < columns.Count; i++)
            columns[i].HeaderIndex = i;
    }

    private int GetSheetDataMemberArrayMaxLength(SheetFieldInfo sheetDataFieldInfo)
    {
        var maxLength = 0;
        foreach (var rowData in RowDataList)
        {
            var arrayValue = sheetDataFieldInfo.GetValue(rowData) as Array;
            if (arrayValue != null)
                maxLength = Mathf.Max(maxLength, arrayValue.Length);
        }

        return maxLength;
    }

    private void AppendSheetColumnsByNestedArray(Type memberType, SheetFieldInfo sheetDataFieldInfo, List<SheetColumnHeader> columns)
    {
        var arrayMaxLength = GetSheetDataMemberArrayMaxLength(sheetDataFieldInfo);
        // add an extra column!
        arrayMaxLength += 1;
        
        var arrayElementType = memberType.GetElementType();
        
        for (int listIndex = 0; listIndex < arrayMaxLength; listIndex++)
        {
            if(EditorSheetTypeUtility.IsNestedType(arrayElementType))
                AppendSheetColumnsByNestedData(arrayElementType, sheetDataFieldInfo, columns, true, listIndex);
            else 
                columns.Add(new SheetColumnHeader(this, sheetDataFieldInfo, true, listIndex, false, default));
        }
    }

    private void AppendSheetColumnsByNestedData(Type resourceType, SheetFieldInfo sheetDataFieldInfo, List<SheetColumnHeader> columns, bool isListColumn = false, int listIndex = default)
    {
        var sheetFields = EditorSheetTypeUtility.GetSheetFields(resourceType);
        foreach (var nestedFieldInfo in sheetFields)
        {
            columns.Add(new SheetColumnHeader(this, sheetDataFieldInfo, isListColumn, listIndex, true, nestedFieldInfo));
        }
    }

    #endregion
    
    #region [Undo/Redo]

    /// <summary>
    /// Save whole sheet text for History;
    /// I once considered using DiffMatchPatch to save memory, but it's a Editor plugin so memory is not important.
    /// </summary>
    public class EditorSheetReaderHistory
    {
        public const int MaxHistory = 100;
        public string Action;
        public string Text;
        public EditorSheetReaderHistory Prev;
        public EditorSheetReaderHistory Next;
    }
    
    private EditorSheetReaderHistory _history;

    public bool CanUndo => _history != null && _history.Prev != null;
    public bool CanRedo => _history != null &&  _history.Next != null;

    private void TryInitHistory()
    {
        if (_history == null)
        {
            SheetLogger.LogDebug("InitHistory");
            var newHistory = new EditorSheetReaderHistory();
            newHistory.Text = CachedJsonText;
            _history = newHistory;
            OnHistoryChange?.Invoke();
        }
    }
    
    private void TryAppendHistory(string action)
    {
        if (_history == null)
        {
            SheetLogger.LogError("History is not initialized!");
            return;
        }

        if(_history.Text == CachedJsonText)
            return;

        SheetLogger.Log("Edit : " + action);
        var newHistory = new EditorSheetReaderHistory();
        newHistory.Action = action;
        newHistory.Text = CachedJsonText;
        _history.Next = newHistory;
        newHistory.Prev = _history;
        _history = newHistory;

        // Limit history count
        var prev = _history.Prev;
        var historyCount = 0;
        while (prev != null)
        {
            historyCount++;
            if (historyCount >= EditorSheetReaderHistory.MaxHistory)
            {
                prev.Prev = null;
                break;
            }

            prev = prev.Prev;
        }

        OnHistoryChange?.Invoke();
    }
 
    public void UndoHistory()
    {
        if (!CanUndo)
        {
            SheetLogger.LogError("Cant undo!");
            return;
        }

        SheetLogger.Log("Undo : " + _history.Action);
        _history = _history.Prev;
        ApplyCurrentHistory();
        OnHistoryChange?.Invoke();
    }

    public void RedoHistory()
    {
        if (!CanRedo)
        {
            SheetLogger.LogError("Cant redo!");
            return;
        }
        
        _history = _history.Next;
        SheetLogger.Log("Redo : " + _history.Action);
        ApplyCurrentHistory();
        OnHistoryChange?.Invoke();
    }

    private void ApplyCurrentHistory()
    {
        SheetDatabase.TryWriteSheetText(SheetDataType.Name, _history.Text);
        ReadSheet();
        HandleDataChange(true);
    }

    public void GetHistoryCount(out int prevCount, out int nextCount)
    {
        prevCount = 0;
        nextCount = 0;
        var prev = _history?.Prev;
        while (prev != null)
        {
            prevCount++;
            prev = prev.Prev;
        }

        var next = _history?.Next;
        while (next != null)
        {
            nextCount++;
            next = next.Next;
        }
    }

    #endregion
}

#endif 