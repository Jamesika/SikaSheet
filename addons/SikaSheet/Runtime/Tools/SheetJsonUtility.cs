using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;

namespace SikaSheet;

/// <summary>
/// use System.Text.Json rather than Json.Net : https://github.com/godotengine/godot/issues/78513
/// </summary>
public static class SheetJsonUtility
{
    private static JsonSerializerOptions _serializeOptions = new JsonSerializerOptions
    {
        IgnoreReadOnlyProperties = true,
        IncludeFields = true,
        
        // Disable for smallest file size
        WriteIndented = true,
    };

    public static string ToJsonForSingleData(SheetData sheetData)
    {
        var json = JsonSerializer.Serialize(sheetData, sheetData.GetType(), _serializeOptions);
        return json;
    }

    public static SheetData FromJsonForSingleData(string json, Type sheetDataType)
    {
        var sheetData = JsonSerializer.Deserialize(json, sheetDataType, _serializeOptions) as SheetData;
        return sheetData;
    }

    public static string ToJson(List<SheetData> dataList, Type sheetDataType)
    {
        var listType = typeof(List<>);
        listType = listType.MakeGenericType(sheetDataType);

        var listT = Activator.CreateInstance(listType) as IList;
        foreach (var sheetData in dataList)
            listT!.Add(sheetData);
        
        var json = JsonSerializer.Serialize(listT, listType, _serializeOptions);
        return json;
    }

    public static List<SheetData> FromJson(string json, Type sheetDataType)
    {
        if (string.IsNullOrEmpty(json))
            return new List<SheetData>();

        var listType = typeof(List<>);
        listType = listType.MakeGenericType(sheetDataType);
        var list = JsonSerializer.Deserialize(json, listType, _serializeOptions) as IList;
        var result = new List<SheetData>();
        if (list != null)
        {
            for (var i = 0; i < list.Count; i++)
                result.Add((SheetData)list[i]);
        }

        return result;
    }
}