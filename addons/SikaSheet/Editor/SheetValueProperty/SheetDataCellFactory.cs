#if TOOLS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace SikaSheet;

[AttributeUsage(AttributeTargets.Class)]
public class SheetValueDefineAttribute : Attribute
{
    public Type ValueType;
    
    public SheetValueDefineAttribute(Type valueType)
    {
        ValueType = valueType;
    }
}  

public class SheetDataCellFactory
{
    private Dictionary<Type, Stack<SheetDataCell>> _pool = new();
    
    private static Dictionary<Type, Type> _dataTypeToControlType;
    private static Dictionary<Type, Type> DataTypeToControlType
    {
        get
        {
            if (_dataTypeToControlType == null)
            {
                _dataTypeToControlType = new();
                var types = Assembly.GetAssembly(typeof(SheetValueProperty))!.GetTypes();
                var valuePropertyTypes =
                    types.Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(SheetValueProperty))).ToList();
                
                foreach (var valuePropertyType in valuePropertyTypes)
                {
                    var attributes = valuePropertyType.GetCustomAttributes<SheetValueDefineAttribute>();
                    foreach (var sheetValueDefineAttribute in attributes)
                    {
                        if (_dataTypeToControlType.ContainsKey(sheetValueDefineAttribute.ValueType))
                        {
                            SheetLogger.LogError("Duplicate defines for edit valueType : " +
                                                 sheetValueDefineAttribute.ValueType.Name);
                            continue;
                        }
                
                        _dataTypeToControlType.Add(sheetValueDefineAttribute.ValueType, valuePropertyType);
                    }
                }
            }  
  
            return _dataTypeToControlType;
        }
    }

    private Control _root;
    private PackedScene _dataCellPackedScene;
    
    public SheetDataCellFactory(Control root, PackedScene dataCellPackedScene)
    {
        _root = root;
        _dataCellPackedScene = dataCellPackedScene;
    }

    public bool IsMatch(SheetColumnHeader header, SheetDataCell dataCell)
    {
        if (dataCell == null)
            return false;
        
        var controlType = GetColumnHeaderValuePropertyType(header);
        return controlType == dataCell.ValuePropertyType;
    }

    public static bool CanEdit(SheetColumnHeader header, out string errorMsg)
    {
        var valuePropertyType = GetColumnHeaderValuePropertyType(header);
        if (valuePropertyType == typeof(InvalidValueProperty))
        {
            errorMsg = $"Can't edit type \"{header.ColumnDataType.Name}\", check:\n" +
                       $"1. Depth of the member\n" +
                       $"2. Type of the member";
            return false;
        } 

        errorMsg = string.Empty;
        return true;
    }

    private static Type GetColumnHeaderValuePropertyType(SheetColumnHeader header)
    {
        if (header.ColumnDataType.IsEnum)
            return typeof(EnumValueProperty);
        
        if (DataTypeToControlType.TryGetValue(header.ColumnDataType, out var controlType))
            return controlType;

        foreach (var kv in DataTypeToControlType)
        {
            var dataType = kv.Key;
            if (header.ColumnDataType.IsAssignableTo(dataType))
                return kv.Value;
        }
        
        return typeof(InvalidValueProperty);
    }

    public SheetDataCell Create(SheetColumnHeader header)
    {
        var controlType = GetColumnHeaderValuePropertyType(header);
        if (!_pool.ContainsKey(controlType))
            _pool.Add(controlType, new Stack<SheetDataCell>());

        if (_pool[controlType].Count == 0)
        {
            var valueProperty = Activator.CreateInstance(controlType) as SheetValueProperty;
            var dataCell = _dataCellPackedScene.Instantiate<SheetDataCell>();
            _root.AddChild(dataCell);
            dataCell.Initialize(valueProperty);
            return dataCell;
        }
        else
        {
            var dataCell = _pool[controlType].Pop();
            dataCell.Visible = true;
            return dataCell;
        }
    }

    public void Recycle(SheetDataCell dataCell)
    {
        dataCell.HandleRecycle();
        if (_pool.TryGetValue(dataCell.ValuePropertyType, out var poolStack))
        {
            poolStack.Push(dataCell);
        }
        else
        {
            SheetLogger.LogError("SheetDataCell Recycle Error!");
            dataCell.QueueFree();
        }
    }
}

#endif