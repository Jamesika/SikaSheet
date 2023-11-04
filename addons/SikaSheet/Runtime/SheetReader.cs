using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SikaSheet;

public abstract class SheetReader
{
    public List<SheetData> RowDataList = new List<SheetData>();
    public readonly Type SheetDataType;
    public readonly string SheetName;

    protected virtual bool EnableCacheJsonText => false;
    protected string CachedJsonText { get; set; }

    public SheetReader(Type sheetDataType)
    {
        SheetDataType = sheetDataType;
        SheetName = sheetDataType.Name;
    }

    public void ReadSheet()
    {
        Stopwatch sw = Stopwatch.StartNew();
        
        SheetDatabase.TryReadSheetText(SheetName, true, out var jsonText, out _);
        var list = SheetJsonUtility.FromJson(jsonText, SheetDataType);
        if (EnableCacheJsonText)
            CachedJsonText = jsonText;
        
        RowDataList.Clear();
        RowDataList.AddRange(list);
        SheetLogger.LogDebug($"Read Sheet {SheetDataType.Name} : {RowDataList.Count} ({sw.ElapsedMilliseconds}ms)");
        
        FixSheetDataIndex();
        OnReadSheet();
    }
    
    protected void FixSheetDataIndex()
    {
        for (var i = 0; i < RowDataList.Count; i++)
            RowDataList[i].Index = i;
    }

    protected abstract void OnReadSheet();
}