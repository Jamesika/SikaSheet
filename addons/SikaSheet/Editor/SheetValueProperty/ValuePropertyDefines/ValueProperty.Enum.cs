#if TOOLS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace SikaSheet;

[Tool]
public partial class EnumValueProperty : SheetValueProperty
{
    private MenuButton _enumOptionButton;

    private List<Enum> _enumValues;
    private bool _isFlagEnum;
    
    public override void _Ready()
    {
        base._Ready();
        _enumOptionButton = new MenuButton();
        _enumOptionButton.Icon = ResourceLoader.Load<Texture2D>(SheetConst.IconPath + "/drop_down.svg");
        AddChildExpandAndFill(_enumOptionButton);
        
        var popup = _enumOptionButton.GetPopup();
        popup.IndexPressed += index =>
        {
            var selectedEnum = _enumValues[(int)index];
            var currentEnum = RawValue as Enum;
            if (currentEnum == null || !_isFlagEnum)
            {
                RawValue = selectedEnum;
            }
            else
            {
                if (currentEnum.HasFlag(selectedEnum))
                {
                    // remove flag
                    var currentEnumValue = Convert.ToUInt64(currentEnum);
                    var selectEnumValue =  Convert.ToUInt64(selectedEnum);
                    currentEnumValue &= ~selectEnumValue;
                    RawValue = Enum.ToObject(DataType, currentEnumValue);
                }
                else
                {
                    // add flag
                    var currentEnumValue = Convert.ToUInt64(currentEnum);
                    var selectEnumValue =  Convert.ToUInt64(selectedEnum);
                    currentEnumValue |= selectEnumValue;
                    RawValue = Enum.ToObject(DataType, currentEnumValue);
                }
            }
        };
    }

    protected override void ShowRawValue()
    {
        _enumValues = Enum.GetValues(DataType).OfType<Enum>().ToList();
        _isFlagEnum = DataType.GetCustomAttribute<FlagsAttribute>() != null;
        
        // options
        var popup = _enumOptionButton.GetPopup();
        popup.Clear();
        var names = Enum.GetNames(DataType);
        var selectedText = "";
        var currentEnum = RawValue as Enum;

        var iconOn = ResourceLoader.Load<Texture2D>(SheetConst.IconPath + "/radio_on.svg");
        var iconOff = ResourceLoader.Load<Texture2D>(SheetConst.IconPath + "/radio_off.svg");
        
        for (var i = 0; i < names.Length; i++)
        {
            var enumName = names[i];
            var enumValue = _enumValues[i];
            popup.AddItem(enumName);
            var isSelected = false;
            if (_isFlagEnum)
                isSelected = currentEnum?.HasFlag(enumValue) ?? false;
            else
                isSelected = enumValue.Equals(currentEnum);
            
            popup.SetItemIcon(i, isSelected ? iconOn : iconOff);
            if (isSelected)
            {
                if (!string.IsNullOrEmpty(selectedText))
                    selectedText += '|';
                selectedText += enumName;
            }
        }
        
        // name 
        _enumOptionButton.Text = selectedText;
    }
}

#endif