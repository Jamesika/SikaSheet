#if TOOLS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SikaSheet;

public static class EditorSheetTypeUtility
{
    public static bool IsNestedType(Type type)
    {
        return type.IsAssignableTo(typeof(SheetNestedData));
    }

    /// <summary>
    /// Support public fields/properties
    /// </summary>
    public static List<SheetFieldInfo> GetSheetFields(Type type)
    {
        return type
            .GetMembers(BindingFlags.Instance | BindingFlags.Public)
            .Where(m =>
            {
                if (m.GetCustomAttribute<IgnoreMemberAttribute>() != null)
                    return false;
                
                if (m is FieldInfo)
                    return true;

                if (m is PropertyInfo propertyInfo
                    && (propertyInfo.SetMethod?.IsPublic ?? false)
                    && (propertyInfo.GetMethod?.IsPublic ?? false))
                    return true;

                return false;
            })
            .Select(m => new SheetFieldInfo(m))
            .ToList();
    }
}


#endif 