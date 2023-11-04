#if TOOLS

using Godot;

namespace SikaSheet;
 
[SheetValueDefine(typeof(Color))]
public partial class ColorValueProperty : SheetValueProperty
{
    private ColorPickerButton _colorPicker;
 
    public override void _Ready()
    {
        base._Ready();
        _colorPicker = new ColorPickerButton();
        AddChildExpandAndFill(_colorPicker);
          
        _colorPicker.PopupClosed += () => SetValue(_colorPicker.Color);
    }

    protected override void ShowRawValue()
    {
        _colorPicker.Color = GetValue<Color>();
    }
}

[SheetValueDefine(typeof(Vector2))]
public partial class Vector2ValueProperty : SheetValueProperty
{
    private NumberEdit _xNumberEdit;
    private NumberEdit _yNumberEdit;
    
    public override void _Ready()
    {
        base._Ready();
        _xNumberEdit = new NumberEdit();
        _yNumberEdit = new NumberEdit();
        AddChildExpandAndFill(_xNumberEdit);
        AddChildExpandAndFill(_yNumberEdit);

        _xNumberEdit.Prefix = "X";
        _yNumberEdit.Prefix = "Y";
        
        _xNumberEdit.MaxValue = float.MaxValue;
        _xNumberEdit.MinValue = float.MinValue;
        _yNumberEdit.MaxValue = float.MaxValue;
        _yNumberEdit.MinValue = float.MinValue;
        
        _xNumberEdit.ValueChanged += newValue => SetValue(new Vector2((float)newValue, GetValue<Vector2>().Y));
        _yNumberEdit.ValueChanged += newValue => SetValue(new Vector2(GetValue<Vector2>().X, (float)newValue));

        _xNumberEdit.LayoutChanged += TriggerLayoutChangeEvent;
        _yNumberEdit.LayoutChanged += TriggerLayoutChangeEvent;
    }

    protected override void ShowRawValue()
    {
        var value = GetValue<Vector2>();
        _xNumberEdit.Value = value.X;
        _yNumberEdit.Value = value.Y;
    }
}

[SheetValueDefine(typeof(Vector2I))]
public partial class Vector2IValueProperty : SheetValueProperty
{
    private NumberEdit _xNumberEdit;
    private NumberEdit _yNumberEdit;

    public override void _Ready()
    {
        base._Ready();
        _xNumberEdit = new NumberEdit();
        _yNumberEdit = new NumberEdit();
        AddChildExpandAndFill(_xNumberEdit);
        AddChildExpandAndFill(_yNumberEdit);

        _xNumberEdit.Prefix = "X";
        _yNumberEdit.Prefix = "Y";
        _xNumberEdit.IsInteger = true;
        _yNumberEdit.IsInteger = true;
        
        _xNumberEdit.MaxValue = int.MaxValue;
        _xNumberEdit.MinValue = int.MinValue;
        _yNumberEdit.MaxValue = int.MaxValue;
        _yNumberEdit.MinValue = int.MinValue;
        
        _xNumberEdit.ValueChanged += newValue => SetValue(new Vector2I(Mathf.RoundToInt(newValue), GetValue<Vector2I>().Y));
        _yNumberEdit.ValueChanged += newValue => SetValue(new Vector2I(GetValue<Vector2I>().X, Mathf.RoundToInt(newValue)));
        
        _xNumberEdit.LayoutChanged += TriggerLayoutChangeEvent;
        _yNumberEdit.LayoutChanged += TriggerLayoutChangeEvent;
    }

    protected override void ShowRawValue()
    {
        var value = GetValue<Vector2I>();
        _xNumberEdit.Value = value.X;
        _yNumberEdit.Value = value.Y;
    }
}

#endif 