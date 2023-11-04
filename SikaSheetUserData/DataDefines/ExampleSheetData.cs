using System;
using Godot;

namespace SikaSheet;

[SheetGroup("C Group")]
public class ExampleSheetDataBasicTypes : SheetData
{
    [SheetKey(0)]
    public string ID;
    public ResourceRef<Texture2D> ResourceA;
    public ResourceRef<PackedScene> ResourceB;
}

[SheetGroup("B Group")]
public class ExampleSheetDataBase : SheetData
{
    [SheetKey(0)]
    public string ID;
    public int IntField;
    public Vector2 Vector2Field;
}

[SheetGroup("B Group")]
public class ExampleSheetDataSubclass : ExampleSheetDataBase
{
    public string StringField;
    public bool BoolField;
}

[SheetGroup("A Group")]
public class ExampleSheetData4 : SheetData
{
    [SheetKey(0)]
    public string ID;
    public int IntField;
    public Vector2 Vector2Field;
}

[SheetGroup("A Group")]
public class ExampleSheetData3 : SheetData
{
    [SheetKey(0)]
    public string ID;
    public int IntField;
    public Vector2 Vector2Field;
}

[SheetGroup]
public class ExampleSheetData2 : SheetData
{
    [SheetKey(0)]
    public string ID;
    public int IntField;
    public Vector2 Vector2Field;
}

public enum TestEnumA
{
    A,
    B,
    C,
    D = A | B,
}

[Flags]
public enum TestEnumB
{
    A = 1<<0,
    B = 1<<1,
    C = 1<<2,
    D = A | B,
}

[SheetGroup]
public class ExampleSheetData : SheetData
{
    [SheetKey(0)] public string ID { get; set; }

    public int[] IntArray { get; set; }
    public Color Color { get; set; }
    public TestEnumA EnumA { get; set; }
    public TestEnumB EnumB { get; set; }
    public ResourceRef<Texture2D> Texture2D { get; set; }

    [SheetKey(1)] public string ID2 { get; set; }
    
    public int IntField { get; set; }
    public int IntProperty { get; set; }
    public string StringField { get; set; }
    public Vector2 Vector2Field { get; set; }
    public ExampleSheetNestedData NestedData { get; set; }
    public ExampleSheetNestedData[] NestedDataArray { get; set; }
}

public class ExampleSheetNestedData : SheetNestedData
{
    public int[] IntArray { get; set; }
    public int IntField { get; set; }
    public string StringField { get; set; }
    public Vector2 Vector2Field { get; set; }
}
