#if TOOLS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Godot;
using FileAccess = Godot.FileAccess;

namespace SikaSheet
{
    public static class SheetCodeGenerateUtility
    {
        private const string TemplateCodeGetSubTable = @"            _allSheetDataList.AddRange({DataName}SheetReader.GetAll());";

        public static void GenerateCode()
        {
            var bindTypes = SheetDatabase.GetSheetDataTypes();
            
            SheetLogger.Log("Begin generate code...");
            var sw = Stopwatch.StartNew();
            
            GenerateCode(bindTypes);
            
            SikaSheetPlugin.Instance.GetEditorInterface().GetResourceFilesystem().Scan();
            SheetLogger.Log($"Generate code success! {sw.ElapsedMilliseconds}ms");
        }

        private static string GetSheetReaderTemplateCode()
        {
            using var fileAccess = FileAccess.Open(SheetConst.TemplateCodePath, FileAccess.ModeFlags.Read);
            return fileAccess.GetAsText();
        }

        private static void GenerateCode(List<Type> bindTypes)
        {
            var generateCodes = new Dictionary<string, string>();
            foreach (var bindType in bindTypes)
            {
                generateCodes.Add($"{bindType.Name}SheetReader.g.cs", GenerateTableCode(bindTypes, bindType));
            }

            var codePath = ProjectSettings.GlobalizePath(SheetConst.GeneratedPath);
            if (Directory.Exists(codePath))
            {
                var files = Directory.GetFiles(codePath);
                foreach (var file in files)
                {
                    var isGenerateCode = file.EndsWith(".g.cs");
                    if(isGenerateCode)
                        File.Delete(file);
                }
            }
            else
            {
                Directory.CreateDirectory(codePath);
            }
            
            foreach (var kv in generateCodes)
            {
                File.WriteAllText(codePath + "/" + kv.Key, kv.Value);
                SheetLogger.Log("Generate : " + kv.Key);
            }
        }  
 
        private static string GenerateTableCode(List<Type> allSheetDataTypes, Type sheetDataType)
        {
            string dataNameStr = sheetDataType.Name;
            string keyTupleStr = string.Empty;
            string keyDefineListStr = string.Empty;
            string keyListStr = string.Empty;
            string createKeyTupleStr = string.Empty;
            string checkKeyEqualsDataStr = string.Empty;
            string keyToStringListStr = string.Empty;
            string getSubTablesStr = string.Empty;

            var keyFieldInfos = GetKeyFieldInfos(sheetDataType);
            if (keyFieldInfos.Count == 1)
            {
                var info = keyFieldInfos[0];
                keyTupleStr = info.FieldType.FullName;
                keyDefineListStr = $"{info.FieldType.FullName} {info.Name}";
                keyListStr = info.Name;
                createKeyTupleStr = info.Name;
                checkKeyEqualsDataStr = $"data.{info.Name} == {info.Name}";
                keyToStringListStr = $"{{{info.Name}}}";
            }
            else 
            {
                keyTupleStr = CreateStringFromFieldInfos(keyFieldInfos, info => info.FieldType.FullName, ",", "({0})");
                keyDefineListStr = CreateStringFromFieldInfos(keyFieldInfos, info => $"{info.FieldType.FullName} {info.Name}");
                keyListStr = CreateStringFromFieldInfos(keyFieldInfos, info => info.Name);
                createKeyTupleStr = CreateStringFromFieldInfos(keyFieldInfos, info => info.Name, ",", "({0})");
                checkKeyEqualsDataStr = CreateStringFromFieldInfos(keyFieldInfos, info => $"data.{info.Name} == {info.Name}", "&& ", "({0})");
                keyToStringListStr = CreateStringFromFieldInfos(keyFieldInfos, info => "{" + info.Name + "}", ",", "({0})");
            }

            getSubTablesStr = CreateStringForGetSubTables(allSheetDataTypes, sheetDataType);

            var template = GetSheetReaderTemplateCode();
            template = template.Replace("{Abstract}", sheetDataType.IsAbstract ? "abstract" : "");
            template = template.Replace("{DataName}", dataNameStr);
            template = template.Replace("{KeyTuple}", keyTupleStr);
            template = template.Replace("{KeyDefineList}", keyDefineListStr);
            template = template.Replace("{CheckKeyEqualsData}", checkKeyEqualsDataStr);
            template = template.Replace("{KeyList}", keyListStr);
            template = template.Replace("{CreateKeyTuple}", createKeyTupleStr);
            template = template.Replace("{KeyToStringList}", keyToStringListStr);
            template = template.Replace("{GetSubTables}", getSubTablesStr);
            return template;
        }

        private static string CreateStringForGetSubTables(List<Type> bindTypes, Type tableDataType)
        {
            var getSubTablesStr = string.Empty;
            foreach (var subTableDataType in bindTypes)
            {
                if (subTableDataType != tableDataType && subTableDataType.IsSubclassOf(tableDataType) && !subTableDataType.IsAbstract)
                {
                    var templateCode = TemplateCodeGetSubTable;
                    templateCode = templateCode.Replace("{DataName}", subTableDataType.Name);
                    getSubTablesStr += templateCode;
                    getSubTablesStr += "\n";
                }
            }

            return getSubTablesStr;
        }

        private static string CreateStringFromFieldInfos(List<SheetFieldInfo> fieldInfos, Func<SheetFieldInfo, string> func, string separator = ",", string format = "{0}")
        {
            string result = string.Empty;
            for (int i = 0; i < fieldInfos.Count; i++)
            {
                var fieldInfo = fieldInfos[i];
                var isFinal = i == fieldInfos.Count - 1;
                var fieldInfoStr = func(fieldInfo);
                result += fieldInfoStr;
                if (!isFinal)
                    result += separator + " ";
            }

            return string.Format(format, result);
        }

        private static List<SheetFieldInfo> GetKeyFieldInfos(Type sheetDataType)
        {
            var fields = EditorSheetTypeUtility.GetSheetFields(sheetDataType);
            var keyFields = fields.Where(f => f.MemberInfo.GetCustomAttribute<SheetKeyAttribute>() != null).ToList();
            keyFields.Sort((a, b) =>
            {
                var aIndex = a.MemberInfo.GetCustomAttribute<SheetKeyAttribute>()!.Index;
                var bIndex = b.MemberInfo.GetCustomAttribute<SheetKeyAttribute>()!.Index;
                if (aIndex == bIndex)
                    SheetLogger.LogError($"{sheetDataType.Name} has same key index {aIndex}!");
                return bIndex - aIndex;
            });
            return keyFields;
        }
    }
}

#endif