#if TOOLS

using Godot;

namespace SikaSheet;

[Tool]
public partial class SheetColumnHeaderCell : SheetCell
{
    [Export] private Label _fieldLabel;
    [Export] private Label _fieldPrefixLabel;
    [Export] private Label _typeLabel;
    [Export] private MenuButton _menuButton;
    [Export] private Control _warningRoot;
    [Export] private TextureRect _warningIcon;

    public SheetColumnHeader Header { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        _menuButton.Pressed += HandleClickMenuButton;
        var popup = _menuButton.GetPopup();
        popup.IdPressed += HandlePopupOnIdPressed;
    }
    
    public void UpdateCell(SheetColumnHeader header)
    {
        Header = header;
        header.GetHeaderDescription(out var prefix, out var memberName);
        _fieldPrefixLabel.Text = prefix;
        _fieldLabel.Text = memberName;

        if (header.IsArrayColumn)
            _typeLabel.Text = $"Array | {GetColumnDataTypeName(header)}";
        else
            _typeLabel.Text = GetColumnDataTypeName(header);

        _menuButton.Disabled = !header.IsArrayColumn;

        //SheetDataCellFactory
        var enableEdit = SheetDataCellFactory.CanEdit(header, out var errorMsg);
        _warningRoot.Visible = !enableEdit;
        _warningIcon.TooltipText = errorMsg;

        IsHighlight = header.IsKey;
    }

    private string GetColumnDataTypeName(SheetColumnHeader header)
    {
        var type = header.ColumnDataType;
        var splits = type.Name.Split('`');
        if (splits.Length == 1)
        {
            return splits[0];
        }
        else
        {
            var genTypes = type.GenericTypeArguments;
            var genStr = "";
            for (var i = 0; i < genTypes.Length; i++)
            {
                if (i != 0)
                    genStr += ",";
                genStr += genTypes[i].Name;
            }

            return $"{splits[0]}<{genStr}>";
        }
    }

    #region [Handle Event]

    private void HandleClickMenuButton()
    {
        var popup = _menuButton.GetPopup();
        popup.Clear();
        popup.AddItem($"Delete Array[{Header.ArrayIndex}]");
        popup.SetItemIcon(0, ResourceLoader.Load<Texture2D>(SheetConst.IconPath + "/delete_pressed.png"));
    }
    
    private void HandlePopupOnIdPressed(long id)
    {
        DataSheetEditor.Instance.SheetReader.DeleteArrayIndex(Header);
    }

    #endregion
}

#endif 