using Godot;

#if TOOLS

namespace SikaSheet;

[Tool]
public abstract partial class SheetCell : MarginContainer
{
    private Vector2 _containerMinSize;

    private bool _isHighlight = false;
    public bool IsHighlight
    {
        get => _isHighlight;
        set
        {
            if(_isHighlight == value)
                return;
            
            _isHighlight = value;
            QueueRedraw();
        }
    }

    private bool _isError;

    public bool IsError
    {
        get => _isError;
        set
        {
            if(_isError == value)
                return;

            _isError = value;
            QueueRedraw();
        }

    }

    public override void _Draw()
    {
        base._Draw();
        var lineWidth = 1f;
        var lineColor = Colors.White * 0.5f;
        var rect = GetGlobalRect();
        var min = GetGlobalTransform().AffineInverse() * rect.Position;
        var max = GetGlobalTransform().AffineInverse() * rect.End;
        DrawLine(max, new Vector2(min.X, max.Y), lineColor, lineWidth);
        DrawLine(max, new Vector2(max.X, min.Y), lineColor, lineWidth);

        if (IsHighlight)
            DrawRect(new Rect2(min, max - min), new Color(0, 1, 0, 0.05f));
        
        if(IsError)
            DrawRect(new Rect2(min, max - min), new Color(1, 0, 0, 0.25f));
    }  
}
 
#endif