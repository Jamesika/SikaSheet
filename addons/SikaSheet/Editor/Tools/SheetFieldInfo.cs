using System;
using System.Reflection;

namespace SikaSheet;

public class SheetFieldInfo
{
    public FieldInfo FieldInfo;
    public PropertyInfo PropertyInfo;
    public MemberInfo MemberInfo;

    public string Name;
    public Type DeclaringType;
    public Type FieldType;

    public bool IsKey => KeyIndex >= 0;
    public int KeyIndex;
    
    public SheetFieldInfo(MemberInfo memberInfo)
    {
        MemberInfo = memberInfo;
        FieldInfo = memberInfo as FieldInfo;
        PropertyInfo = memberInfo as PropertyInfo;

        Name = memberInfo.Name;
        DeclaringType = memberInfo.DeclaringType;
        FieldType = FieldInfo?.FieldType ?? PropertyInfo?.PropertyType;

        var keyAttribute = memberInfo.GetCustomAttribute<SheetKeyAttribute>();
        if (keyAttribute != null)
            KeyIndex = keyAttribute.Index;
        else 
            KeyIndex = -1;

        if (FieldInfo == null && PropertyInfo == null)
            SheetLogger.LogError("Cant create sheet field info : " + memberInfo.Name);
    }

    public object GetValue(object data)
    {
        if (FieldInfo != null)
            return FieldInfo.GetValue(data);
        else
            return PropertyInfo.GetValue(data);
    }

    public void SetValue(object data, object value)
    {
        if (FieldInfo != null)
            FieldInfo.SetValue(data, value);
        else
            PropertyInfo.SetValue(data, value);
    }
}