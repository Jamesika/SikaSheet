using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Godot;
using FileAccess = Godot.FileAccess;

namespace SikaSheet;

public static class SheetDatabase
{
    private static List<Type> _sheetTypes;
    
    public static List<Type> GetSheetDataTypes()
    {
        if (_sheetTypes == null)
        {
            _sheetTypes = new List<Type>();
            var sheetsAssembly = Assembly.GetAssembly(typeof(SheetData));
            var sheetTypes = sheetsAssembly!.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(SheetData)) && !t.IsAbstract)
                .ToList();

            var hashset = new HashSet<string>();
            foreach (var sheetType in sheetTypes)
            {
                if (hashset.Contains(sheetType.Name))
                {
                    SheetLogger.LogError("Sheet name is duplicate : " + sheetType.Name);
                }
                else
                {
                    hashset.Add(sheetType.Name);
                    _sheetTypes.Add(sheetType);
                }
            }
        }

        return _sheetTypes;
    }
  
    public static bool TryReadSheetText(string sheetName, bool loadEmptySheetIfNoData, out string jsonText, out Type sheetType)
    {
        if (TryGetSheetPathByName(sheetName, out var sheetPath, out sheetType))
        {
            using var file = FileAccess.Open(sheetPath, FileAccess.ModeFlags.Read);
            if (file != null)
            {
                jsonText = file.GetAsText();
                return true;
            }
            else if (loadEmptySheetIfNoData)
            {
                jsonText = "";
                return true;
            }
        }

        SheetLogger.LogError("Get sheet text error : " + sheetName);
        jsonText = null;
        sheetType = null;
        return false;
    }

    public static bool TryWriteSheetText(string sheetName, string jsonText)
    {
        if (TryGetSheetPathByName(sheetName, out var sheetPath, out var sheetDataType))
        {
            var systemSheetPath = ProjectSettings.GlobalizePath(sheetPath);
            var directory = Path.GetDirectoryName(systemSheetPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory!);
            
            using var file = FileAccess.Open(sheetPath, FileAccess.ModeFlags.Write);
            file.StoreString(jsonText);
            SheetLogger.LogDebug($"Save sheet {sheetName} at {sheetPath}!");
            return true;
        }

        SheetLogger.LogError("Write sheet data error!");
        return false;
    }

    public static T CreateRuntimeSheetReader<T>(Type dataType) where T : RuntimeSheetReader
    {
        if (TryReadSheetText(dataType.Name, true, out var jsonText, out var _))
        {
            var runtimeSheetReader = Activator.CreateInstance(typeof(T), dataType) as T;
            runtimeSheetReader!.ReadSheet();
            return runtimeSheetReader;
        }

        SheetLogger.LogError("CreateRuntimeSheetReader failed : "+dataType.Name);
        return null;
    }

    private static bool TryGetSheetPathByName(string sheetName, out string sheetPath, out Type sheetType)
    {
        var sheetTypes = GetSheetDataTypes();
        foreach (var type in sheetTypes)
        {
            if (type.Name == sheetName)
            {
                sheetPath = SheetConst.DataSheetPath + "/" + sheetName + ".json";
                sheetType = type;
                return true;
            }
        }

        sheetPath = string.Empty;
        sheetType = null;
        return false;
    }

    #region [Runtime Sheet Reader]

    private static Dictionary<Type, RuntimeSheetReader> _allRuntimeReaders = new();

    public static void Register(RuntimeSheetReader reader)
    {
        var dataType = reader.SheetDataType;
        if (_allRuntimeReaders.ContainsKey(dataType))
        {
            SheetLogger.LogError($"Register same RuntimeSheetReader : {dataType.Name}");
            return;
        }

        _allRuntimeReaders.Add(dataType, reader);
    }

    public static void SetRuntimeSheetReaderDirty(Type sheetDataType)
    {
        foreach (var kv in _allRuntimeReaders)
        {
            var needSetDirty = kv.Key == sheetDataType || sheetDataType.IsSubclassOf(kv.Key);
            if (needSetDirty)
                kv.Value.SetDirty();
        }
    }

    #endregion
}