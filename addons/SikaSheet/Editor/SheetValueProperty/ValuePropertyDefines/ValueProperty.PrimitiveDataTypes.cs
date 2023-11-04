#if TOOLS

using Godot;

namespace SikaSheet;


[SheetValueDefine(typeof(int))]
public partial class IntValueProperty : SheetValueProperty
{
    private NumberEdit _numberEdit;
 
    public override void _Ready()
    {
        base._Ready();
        _numberEdit = new NumberEdit();
        _numberEdit.IsInteger = true;
        _numberEdit.MaxValue = int.MaxValue;
        _numberEdit.MinValue = int.MinValue;
        AddChildExpandAndFill(_numberEdit);
        _numberEdit.ValueChanged += value => SetValue(Mathf.RoundToInt(value));
        _numberEdit.LayoutChanged += TriggerLayoutChangeEvent;
    }

    protected override void ShowRawValue()
    {
        _numberEdit.Value = GetValue<int>();
    }  
}     
  
[SheetValueDefine(typeof(uint))]
public partial class UIntValueProperty : SheetValueProperty
{
    private NumberEdit _numberEdit;

    public override void _Ready()
    {
        base._Ready();
        _numberEdit = new NumberEdit();
        _numberEdit.IsInteger = true;
        _numberEdit.MaxValue = uint.MaxValue;
        _numberEdit.MinValue = uint.MinValue;
        _numberEdit.DisableNegativeNumber = true;
        AddChildExpandAndFill(_numberEdit);
        _numberEdit.ValueChanged += value => SetValue((uint)Mathf.RoundToInt(value));
        _numberEdit.LayoutChanged += TriggerLayoutChangeEvent;
    }

    protected override void ShowRawValue()
    {
        _numberEdit.Value = GetValue<uint>();
    }
}

[SheetValueDefine(typeof(float))]
public partial class FloatValueProperty : SheetValueProperty
{
    private NumberEdit _numberEdit;
    
    public override void _Ready()
    {
        base._Ready();
        _numberEdit = new NumberEdit();
        _numberEdit.MaxValue = float.MaxValue;
        _numberEdit.MinValue = float.MinValue;
        _numberEdit.ValueChanged += newValue => SetValue((float)newValue);
        _numberEdit.LayoutChanged += TriggerLayoutChangeEvent;
        AddChildExpandAndFill(_numberEdit);
    }

    protected override void ShowRawValue()
    {
        _numberEdit.Value = GetValue<float>();
    }
}

[SheetValueDefine(typeof(double))]
public partial class DoubleValueProperty : SheetValueProperty
{
    private NumberEdit _numberEdit;
    
    public override void _Ready()
    {
        base._Ready();
        _numberEdit = new NumberEdit();
        _numberEdit.ValueChanged += newValue => SetValue(newValue);
        _numberEdit.LayoutChanged += TriggerLayoutChangeEvent;
        AddChildExpandAndFill(_numberEdit);
    }
    
    protected override void ShowRawValue()
    {
        _numberEdit.Value = GetValue<double>();
    }
}


[SheetValueDefine(typeof(string))]
public partial class StringValueProperty : SheetValueProperty, ISerializationListener
{
    private LineEdit _lineEdit;
    
    public override void _Ready()
    {
        base._Ready();
        _lineEdit = new LineEdit();
        AddChildExpandAndFill(_lineEdit);
        _lineEdit.ExpandToTextLength = true;
        _lineEdit.Flat = true;
        _lineEdit.TextSubmitted += text => SetValue(text);
        _lineEdit.FocusExited += HandleFocusExited;
        _lineEdit.TextChanged += _ => TriggerLayoutChangeEvent();
    }

    private void HandleFocusExited()
    {
        SheetLogger.LogDebug("StringValueProperty OnFocusExited");
        SetValue(_lineEdit.Text);
    }

    protected override void ShowRawValue()
    {
        if (GetValue<string>() == null)
            _lineEdit.Text = string.Empty;
        else 
            _lineEdit.Text = GetValue<string>();
    }

    public void OnBeforeSerialize()
    {
        // fix errors when Build C# in editor 
        _lineEdit.FocusExited -= HandleFocusExited;
    }

    public void OnAfterDeserialize()
    {
        _lineEdit.FocusExited += HandleFocusExited;
    }
}

[SheetValueDefine(typeof(bool))]
public partial class BoolValueProperty : SheetValueProperty
{
    private CheckBox _checkBox;
    
    public override void _Ready()
    {
        base._Ready();
        _checkBox = new CheckBox();
        AddChildExpandAndFill(_checkBox);
        _checkBox.Toggled += value => SetValue(value);
    }
    
    protected override void ShowRawValue()
    {
        _checkBox.ButtonPressed = GetValue<bool>();
    }
}

#endif 