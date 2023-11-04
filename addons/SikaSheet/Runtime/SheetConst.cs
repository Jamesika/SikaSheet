using Godot;
using System;

namespace SikaSheet;

public static class SheetConst
{
    public const string UserDataPath = "res://SikaSheetUserData";
    public const string DataSheetPath = UserDataPath +"/Sheets";
    public const string GeneratedPath = UserDataPath + "/Generated";
    
    public const string TemplateCodePath = "res://addons/SikaSheet/Resources/SheetReaderCodeTemplate.txt";

    public const string IconPath = "res://addons/SikaSheet/Resources/Icons";
    public const string MainScenePath = "res://addons/SikaSheet/Resources/PackedScenes/data_sheet_editor.tscn";
}
  