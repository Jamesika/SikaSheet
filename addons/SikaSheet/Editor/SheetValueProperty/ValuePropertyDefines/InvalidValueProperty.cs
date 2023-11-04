
#if TOOLS

using Godot;

namespace SikaSheet;

public partial class InvalidValueProperty : SheetValueProperty
{
    private Label _label;

    public override void _Ready()
    {
        base._Ready();
        _label = new Label();
        AddChildExpandAndFill(_label);
    }

    protected override void ShowRawValue()
    {
        _label.Text = RawValue?.ToString() ?? "NULL";
    }
} 

#endif 