#if TOOLS

using System;
using System.Text;

namespace SikaSheet;

public class SheetColumnHeader
{
    public EditorSheetReader EditorSheetReader { get; private set; }

    public int HeaderIndex;
    public Type ColumnDataType;
    public int BaseTypeDepth = 0;

    #region [Member Info]
    
    public SheetFieldInfo SheetDataFieldInfo;
    public bool IsArrayColumn;
    public int ArrayIndex;
    public bool IsNestedDataColumn;
    public SheetFieldInfo NestedFieldInfo;
    
    #endregion

    public bool IsKey => SheetDataFieldInfo.IsKey;
    public int KeyIndex => SheetDataFieldInfo.KeyIndex;
    
    public SheetColumnHeader(EditorSheetReader editorSheetReader, SheetFieldInfo sheetDataFieldInfo, bool isArrayColumn, int arrayIndex, bool isNestedDataColumn, SheetFieldInfo nestedFieldInfo)
    {
        EditorSheetReader = editorSheetReader;

        // Calculate priority
        var baseTypeDepth = 0;
        var baseType = sheetDataFieldInfo.DeclaringType;
        while (baseType != null)
        {
            baseType = baseType.BaseType;
            baseTypeDepth++;
        }
        BaseTypeDepth = baseTypeDepth;
        
        SheetDataFieldInfo = sheetDataFieldInfo;
        IsArrayColumn = isArrayColumn;
        ArrayIndex = arrayIndex;
        IsNestedDataColumn = isNestedDataColumn;
        NestedFieldInfo = nestedFieldInfo;

        if (isNestedDataColumn)
            ColumnDataType = NestedFieldInfo.FieldType;
        else if (IsArrayColumn)
            ColumnDataType = SheetDataFieldInfo.FieldType.GetElementType();
        else
            ColumnDataType = SheetDataFieldInfo.FieldType;
    }

    /// <summary>
    /// get Array rather than Array element;
    /// </summary>
    private Array GetFinalArray(SheetData rowData)
    {
        if (!IsArrayColumn || IsNestedDataColumn)
        {
            SheetLogger.LogError("Cant GetFinalArray");
            return null;
        }
        
        var fieldValue = SheetDataFieldInfo.GetValue(rowData);
        return fieldValue as Array;
    }
    
    /// <summary>
    /// get Data rather than Data field;
    /// </summary>
    private object GetFinalData(SheetData rowData, out SheetFieldInfo dataFieldInfo, out bool isOutOfArrayRange, out bool enableAddArrayElement)
    {
        isOutOfArrayRange = false;
        enableAddArrayElement = false;
        dataFieldInfo = null;

        if (IsArrayColumn && !IsNestedDataColumn)
        {
            SheetLogger.LogError("Cant GetFinalData");
            return null;
        }

        if (!IsNestedDataColumn && !IsArrayColumn)
        {
            dataFieldInfo = SheetDataFieldInfo;
            return rowData;
        }
        
        var fieldValue = SheetDataFieldInfo.GetValue(rowData);
        if (fieldValue == null)
            return null;

        if (IsArrayColumn)
        {
            var array = fieldValue as Array;
            if (ArrayIndex >= array!.Length)
            {
                isOutOfArrayRange = true;
                enableAddArrayElement = ArrayIndex == array.Length;
                return null;
            }

            fieldValue = array.GetValue(ArrayIndex);
        }
        
        dataFieldInfo = NestedFieldInfo;
        return fieldValue;
    }

    public bool Validate(SheetData rowData)
    {
        var dataValue = GetDataValue(rowData, out _, out _, out _);
        if (dataValue is IResourceRef resourceRef)
        {
            if (!resourceRef.Validate())
                return false;
        }

        return true;
    }

    public object GetDataValue(SheetData rowData, out bool isOutOfArrayRange, out bool enableAddArrayElement, out bool isNestedDataNull)
    {
        isOutOfArrayRange = false;
        enableAddArrayElement = false;
        isNestedDataNull = false;
        
        if (!IsNestedDataColumn && IsArrayColumn)
        {
            var array = GetFinalArray(rowData);
            if (array == null)
            {
                isNestedDataNull = true;
                return null;
            }

            if (array.Length <= ArrayIndex)
            {
                isOutOfArrayRange = true;
                enableAddArrayElement = array.Length == ArrayIndex;
                return null;
            }

            return array.GetValue(ArrayIndex);
        }
        else
        {
            var data = GetFinalData(rowData, out var dataFieldInfo, out isOutOfArrayRange, out enableAddArrayElement);
            if (data == null)
            {
                isNestedDataNull = true;
                return null;
            }

            return dataFieldInfo.GetValue(data);
        }
    }

    public void GetHeaderDescription(out string prefix, out string fieldName)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(SheetDataFieldInfo.Name);
        if (IsArrayColumn)
            sb.Append($"[{ArrayIndex}]");
        
        if (IsNestedDataColumn)
        {
            sb.Append('.');
            prefix = sb.ToString();
            fieldName = NestedFieldInfo.Name;
        }
        else
        {
            prefix = string.Empty;
            fieldName = sb.ToString();
        }
    }

    #region [Modify Sheet]

    public bool SetDataValue(SheetData rowData, object newValue)
    {
        var currentValue = GetDataValue(rowData, out var isOutOfArrayRange, out _, out var isNestedDataNull);
        if (isOutOfArrayRange || isNestedDataNull)
        {
            SheetLogger.LogError("SetDataValue error!");
            return false;
        }

        if (!Equals(currentValue, newValue))
        {
            if (!IsNestedDataColumn && IsArrayColumn)
            {
                var array = GetFinalArray(rowData);
                array.SetValue(newValue, ArrayIndex);
                return true;
            }
            else
            {
                var data = GetFinalData(rowData, out var dataFieldInfo, out _, out _);
                dataFieldInfo.SetValue(data, newValue);
                return true;
            }
        }

        return false;
    }
  
    public void AddArrayElement(SheetData rowData)
    {
        if(!IsArrayColumn)
        {
            SheetLogger.LogError("AddArrayElement error!");
            return;
        }

        var fieldValue = SheetDataFieldInfo.GetValue(rowData);
        if (fieldValue == null)
        {
            SheetLogger.LogError("AddArrayElement error : array is null!");
            return;
        }

        // Resize Array
        var arrayElementType = SheetDataFieldInfo.FieldType.GetElementType();
        var array = fieldValue as Array;
        var newArray = Array.CreateInstance(arrayElementType!, array!.Length + 1);
        Array.Copy(array, newArray, array.Length);
        // Fill null data for array
        if (EditorSheetTypeUtility.IsNestedType(arrayElementType))
        {
            var newNestedData = Activator.CreateInstance(arrayElementType);
            newArray.SetValue(newNestedData, newArray.Length - 1);
        }

        SheetDataFieldInfo.SetValue(rowData, newArray);
        SheetLogger.LogDebug("Add Array Element");
    }
 
    public void DeleteArrayElement(SheetData rowData)
    {
        if(!IsArrayColumn)
        {
            SheetLogger.LogError("DeleteArrayElement error!");
            return;
        }

        var fieldValue = SheetDataFieldInfo.GetValue(rowData);
        if (fieldValue == null)
            return;
        
        // Resize Array
        var arrayElementType = SheetDataFieldInfo.FieldType.GetElementType();
        var array = fieldValue as Array;
        if (array!.Length <= ArrayIndex)
            return;

        var removeAtIndex = ArrayIndex;
        var newArray = Array.CreateInstance(arrayElementType!, array!.Length - 1);
        for (int index = 0; index < array.Length; index++)
        {
            if (index < removeAtIndex)
                newArray.SetValue(array.GetValue(index), index);
            else if (index > removeAtIndex)
                newArray.SetValue(array.GetValue(index), index - 1);
        }

        SheetDataFieldInfo.SetValue(rowData, newArray);
        SheetLogger.LogDebug("Delete Array Element");
    }

    public bool FixNullData(SheetData rowData)
    {
        var fieldValue = SheetDataFieldInfo.GetValue(rowData);
        if (fieldValue == null)
        {
            if (IsArrayColumn)
            {
                // create array
                var elementType = SheetDataFieldInfo.FieldType.GetElementType();
                var newArray = Array.CreateInstance(elementType!, 0);
                SheetDataFieldInfo.SetValue(rowData, newArray);
                return true;
            }
            else if (IsNestedDataColumn)
            {
                // create nested data
                var newNestedData = Activator.CreateInstance(SheetDataFieldInfo.FieldType);
                SheetDataFieldInfo.SetValue(rowData, newNestedData);
                return true;
            }
        }

        if (IsArrayColumn && IsNestedDataColumn)
        {
            var array = fieldValue as Array;
            if (array!.Length > ArrayIndex)
            {
                var arrayValue = array.GetValue(ArrayIndex);
                if (arrayValue == null)
                {
                    // create nested data
                    var newNestedData = Activator.CreateInstance(SheetDataFieldInfo.FieldType.GetElementType()!);
                    array.SetValue(newNestedData, ArrayIndex);
                    return true;
                }
            }
        }

        return false;
    }

    public bool FixResource(SheetData rowData)
    {
        var dataValue = GetDataValue(rowData, out _, out _, out _);
        if (dataValue is IResourceRef resourceRef)
        {
            if (resourceRef.TryFixResource())
            {
                SetDataValue(rowData, resourceRef);
                return true;
            }
        }

        return false;
    }

    #endregion
}

#endif