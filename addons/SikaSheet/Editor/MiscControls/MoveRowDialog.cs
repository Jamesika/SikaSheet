#if TOOLS

using Godot;

namespace SikaSheet;

[Tool]
public partial class MoveRowDialog : ConfirmationDialog
{
    private SpinBox _spinBox;
    private int _startRowIndex;
    private EditorSheetReader _reader;
    
    public void Setup(int startRowIndex, EditorSheetReader reader)
    {
        this.InitialPosition = WindowInitialPosition.CenterMainWindowScreen;
        this.Size = new Vector2I(400, 50);
        
        _startRowIndex = startRowIndex;
        _reader = reader;
        
        _spinBox = new SpinBox();
        _spinBox.Rounded = true;
        _spinBox.MinValue = 0;
        _spinBox.MaxValue = reader.RowDataList.Count;
        AddChild(_spinBox);

        Title = $"Move Row {startRowIndex} to [0~{reader.RowDataList.Count}]";
        Confirmed += HandleConfirmed;
    }

    private void HandleConfirmed()
    {
        _reader.MoveRow(_startRowIndex, Mathf.RoundToInt(_spinBox.Value));
    }
}

#endif 