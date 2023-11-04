#if TOOLS

using System;
using Godot;

namespace SikaSheet;

[Tool]
public partial class SheetIndexCell : SheetCell
{
    [Export] private TextureButton _deleteRowButton;
    [Export] private MenuButton _indexMenuButton;
    private int _rowIndex;

    private const int CopyRowKey = 1;
    private const int PasteRowKey = 2;
    private const int DuplicateRowKey = 3;
    private const int MoveRowKey = 4;
    private const int DeleteRowKey = 5;
    
    public override void _Ready()
    {
        base._Ready();
        _deleteRowButton.Pressed += HandleClickDeleteRowButton;
        _indexMenuButton.Pressed += HandleClickMenuButton;
        var popup = _indexMenuButton.GetPopup();
        popup.IdPressed += HandlePopupOnIdPressed;
    }

    public void UpdateCell(int index)
    {
        _rowIndex = index;
        _indexMenuButton.Text = index.ToString();
        _deleteRowButton.Visible = DataSheetEditor.Instance.EnableQuickDeleteRow;
        
        // highlight 
        IsHighlight = DataSheetEditor.Instance.SheetReader.HighlightRow == index;
    }

    public override void _GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);
        if (@event is InputEventMouseButton eventMouseButton 
            && eventMouseButton.ButtonIndex == MouseButton.Left 
            && !eventMouseButton.IsEcho() 
            && eventMouseButton.Pressed)
        {
            DataSheetEditor.Instance.SheetReader.SetHighlightRow(_rowIndex);
        }
    }

    #region [Handle Events]

    private void HandleClickDeleteRowButton()
    {
        DataSheetEditor.Instance.SheetReader?.DeleteRow(_rowIndex);
    }

    private void HandleClickMenuButton()
    {
        var popup = _indexMenuButton.GetPopup();
        popup.Clear();
        popup.AddItem($"Copy Row", CopyRowKey);
        popup.AddItem($"Paste Row", PasteRowKey);
        popup.AddSeparator();
        popup.AddItem($"Duplicate Row", DuplicateRowKey);
        popup.AddItem($"Move Row", MoveRowKey);
        popup.AddSeparator();
        popup.AddItem($"Delete Row", DeleteRowKey);
        
        popup.SetItemIcon(0, ResourceLoader.Load<Texture2D>(SheetConst.IconPath + "/copy.svg"));
        popup.SetItemIcon(1, ResourceLoader.Load<Texture2D>(SheetConst.IconPath + "/paste.svg"));
        // separator
        popup.SetItemIcon(3, ResourceLoader.Load<Texture2D>(SheetConst.IconPath + "/duplicate.svg"));
        popup.SetItemIcon(4, ResourceLoader.Load<Texture2D>(SheetConst.IconPath + "/move.svg"));
        // separator
        popup.SetItemIcon(6, ResourceLoader.Load<Texture2D>(SheetConst.IconPath + "/delete_pressed.png"));
        
        popup.SetItemDisabled(1,  !DataSheetEditor.Instance.SheetReader.HasCopiedRow());
        //popup.SetItemDisabled(1,  true);
    }
    
    private void HandlePopupOnIdPressed(long id)
    {
        switch (id)
        {
            case CopyRowKey:
                DataSheetEditor.Instance.SheetReader.CopyRow(_rowIndex);
                break;
            case PasteRowKey:
                DataSheetEditor.Instance.SheetReader.PasteRow(_rowIndex);
                break;
            case DuplicateRowKey:
                DataSheetEditor.Instance.SheetReader.DuplicateRow(_rowIndex);
                break;
            case MoveRowKey:
                HandleMoveRow();
                break;
            case DeleteRowKey:
                DataSheetEditor.Instance.SheetReader.DeleteRow(_rowIndex);
                break;
        }
    }

    private void HandleMoveRow()
    {
        MoveRowDialog dialog = new MoveRowDialog();
        dialog.Setup(_rowIndex, DataSheetEditor.Instance.SheetReader);
        AddChild(dialog);
        dialog.Show();
    }

    #endregion
 
}

#endif 