#if TOOLS

using System;
using System.Linq;
using Godot;

namespace SikaSheet;

#region [Base]

[Tool]
public abstract partial class SheetValueProperty : HBoxContainer
{
    public event Action<object> ValueChanged;
    public event Action LayoutChanged;  

    private bool _hasBeenSet;
    private object _rawValue;

    public Control FirstFocusable { get; private set; }
    public Control LastFocusable { get; private set; }

    public Type DataType { get; set; }

    public object RawValue
    {
        get => _rawValue;
        set
        { 
            if (!_hasBeenSet || !Equals(_rawValue, value))
            {
                _rawValue = value;
                HandleValueChanged();
            }  
        }
    }

    private bool _isReady = false;
    
    public override void _Ready()
    {
        base._Ready();
        this.SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill;
        this.SizeFlagsVertical = SizeFlags.Expand | SizeFlags.Fill;
        _isReady = true;
    }

    public void SetupFocus()
    {
        if (!_isReady)
        {
            SheetLogger.LogError("ValueProperty is not ready!");
            return;
        }

        foreach (var child in GetChildren())
        {
            var controlChild = child as Control;
            if(controlChild == null)
                continue;

            if (controlChild.FocusMode == FocusModeEnum.All)
            {
                if (FirstFocusable == null)
                    FirstFocusable = controlChild;

                LastFocusable = controlChild;
            }
            
            foreach (var innerChild in controlChild.GetChildren())
            {
                if (innerChild is Control innerControlChild && innerControlChild.FocusMode == FocusModeEnum.All)
                {
                    if (FirstFocusable == null)
                        FirstFocusable = innerControlChild;

                    LastFocusable = innerControlChild;
                }
            }
        }
    }

    protected void AddChildExpandAndFill(Control control)
    {
        control.SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill;
        control.SizeFlagsVertical = SizeFlags.Expand | SizeFlags.Fill;
        AddChild(control);
    }

    public T GetValue<T>()
    {
        if (RawValue == null)
            return default;

        return (T)RawValue;
    }

    public void SetValue<T>(T value)
    {
        RawValue = value;
    }

    protected void HandleValueChanged()
    {
        _hasBeenSet = true;
        ShowRawValue();
        ValueChanged?.Invoke(RawValue);
    }

    protected abstract void ShowRawValue();

    protected void TriggerLayoutChangeEvent()
    {
        LayoutChanged?.Invoke();
    }
}
 
#endregion
#endif 