using System;

namespace SikaSheet;

[AttributeUsage(AttributeTargets.Class)]
public class SheetGroupAttribute : Attribute
{
    public string GroupName;

    public SheetGroupAttribute(string groupName = null)
    {
        GroupName = groupName;
    }
}


[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SheetKeyAttribute : Attribute
{
    public int Index;
    public SheetKeyAttribute(int index)
    {
        Index = index;
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class IgnoreMemberAttribute : Attribute
{
}

/// <summary>
/// This kind of data is nested in SheetData 
/// </summary>
public abstract class SheetNestedData
{
}

public abstract class SheetData
{
    [IgnoreMember]
    public int Index;
}
