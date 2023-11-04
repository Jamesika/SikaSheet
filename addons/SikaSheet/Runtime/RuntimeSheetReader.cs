using System;
using System.Collections.Generic;

namespace SikaSheet;


public abstract class RuntimeSheetReader : SheetReader
{
    private bool _isDirty;

    public RuntimeSheetReader(Type sheetDataType) : base(sheetDataType)
    {
        SheetDatabase.Register(this);
    }

    public void SetDirty()
    {
        SheetLogger.LogDebug($"RuntimeSheetReader {GetType().Name} SetDirty");
        _isDirty = true;
    }

    protected void EnsureSheetDataNotDirty()
    {
        if (!_isDirty)
            return;
  
        _isDirty = false;

        SheetLogger.LogDebug("Reload sheet data : " + SheetDataType.Name);
        // reload sheet
        ClearCache();
        ReadSheet();
    }

    protected virtual void ClearCache() { }
}

public abstract class RuntimeSheetReader<T> : RuntimeSheetReader where T : SheetData
{
    public List<T> RowDataListT = new List<T>();

    protected override void OnReadSheet()
    {
        RowDataListT.Clear();
        foreach (var sheetData in RowDataList)
        {
            var sheetDataT = sheetData as T;
            if (sheetDataT != null)
                RowDataListT.Add(sheetDataT);
            else
                SheetLogger.LogError("SheetDataT is null");
        }
    }

    protected RuntimeSheetReader(Type sheetDataType) : base(sheetDataType)
    {
    }
}