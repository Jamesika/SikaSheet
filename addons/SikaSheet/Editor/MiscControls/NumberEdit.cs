#if TOOLS

using System;
using Godot;

namespace SikaSheet;

/// <summary>
/// like SpinBox
/// </summary>
[Tool]
public partial class NumberEdit : HBoxContainer, ISerializationListener
{
    private Label _prefixLabel;
    private LineEdit _lineEdit;

    public bool IsInteger { get; set; }
    public bool DisableNegativeNumber { get; set; }

    public double MaxValue { get; set; } = double.MaxValue;
    public double MinValue { get; set; } = double.MinValue;

    public event Action<double> ValueChanged;
    public event Action LayoutChanged;

    private double _value;
    
    public double Value
    {
        get => _value;
        set
        {
            if (IsInteger)
                value = Mathf.Round(value);

            if (DisableNegativeNumber)
                value = Mathf.Max(0, value);

            // Json cant serialize larger number!
            value = Mathf.Clamp(value, float.MinValue, float.MaxValue);

            value = Mathf.Clamp(value, MinValue, MaxValue);
            if (double.IsInfinity(value) || double.IsNaN(value))
                value = _value;

            if (!Mathf.IsEqualApprox(_value, value))
            {
                SetValueWithoutNotify(value);
                ValueChanged?.Invoke(_value);
            }
            
            SetValueWithoutNotify(value);
        }
    }

    private string _prefix;

    public string Prefix
    {
        get => _prefix;
        set
        {
            _prefix = value;
            SetValueWithoutNotify(Value); 
        }
    }

    public override void _Ready()
    {
        base._Ready();
        _prefixLabel = new Label();
        AddChild(_prefixLabel);

        _lineEdit = new LineEdit();
        _lineEdit.ExpandToTextLength = true;
        _lineEdit.Flat = true;
        _lineEdit.SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill;
        _lineEdit.SizeFlagsVertical = SizeFlags.Expand | SizeFlags.Fill;
        AddChild(_lineEdit);

        _lineEdit.TextChanged += _ => LayoutChanged?.Invoke();
        _lineEdit.TextSubmitted += text =>
        {
            HandleTextChange(text);
            LayoutChanged?.Invoke();
        };  
        _lineEdit.FocusExited += HandleFocusExited;
        
        SetValueWithoutNotify(0);
    }

    private bool IsPrepared()
    {
        return IsInstanceValid(_prefixLabel) && IsInstanceValid(_lineEdit);
    }

    private void SetValueWithoutNotify(double value)
    {
        // fix errors after build C# in editor 
        if(!IsPrepared())
            return;
        
        if (IsInteger)
            _value = Mathf.Round(value);
        else
            _value = value;

        // prefix
        var hasPrefix = !string.IsNullOrEmpty(_prefix);
        _prefixLabel.Visible = hasPrefix;
        if (hasPrefix)
            _prefixLabel.Text = _prefix;
        else
            _prefixLabel.Text = string.Empty;

        // value
        if (IsInteger)
            _lineEdit.Text = Mathf.RoundToInt(_value).ToString();
        else
            _lineEdit.Text = _value.ToString();
    }

    private void HandleFocusExited()
    {
        HandleTextChange(_lineEdit.Text);
        LayoutChanged?.Invoke();
    }
    
    private void HandleTextChange(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Value = default;
            return;
        }

        if (!double.TryParse(text, out var doubleValue))
        {
            SetValueWithoutNotify(Value);
            return;
        }

        Value = doubleValue;
    }

    public void OnBeforeSerialize()
    {
        _lineEdit.FocusExited -= HandleFocusExited;
    }

    public void OnAfterDeserialize()
    {
        _lineEdit.FocusExited += HandleFocusExited;
        SetValueWithoutNotify(Value);
    } 
}         

#endif