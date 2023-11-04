#if TOOLS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Godot;

namespace SikaSheet;

[Tool]
public partial class DataSheetEditor : Control
{
    public static DataSheetEditor Instance { get; private set; }
    public EditorSheetReader SheetReader { get; private set; }
    public string CurrentSheetName { get; private set; }

    private bool _enableQuickDeleteRow;
    public bool EnableQuickDeleteRow
    {
        get => _enableQuickDeleteRow;
        set
        {
            _enableQuickDeleteRow = value;
            _sheetView?.SetDirty();
        }
    }

    private bool _fileSystemDirtyFlag = false;
    
    #region [Exports]

    [Export] private Tree _sheetsItemTree;
    [Export] private SheetView _sheetView;
    [Export] private Control _hasSheetRoot;
    [Export] private Control _emptySheetRoot;
    [Export] private MenuButton _mainMenuButton;
    [Export] private Label _historyLabel;

    public SheetView SheetView => _sheetView;
    
    #endregion

    #region [Sheet Exports]

    [Export] private Label _sheetClassNameLabel;
    [Export] private Label _sheetBaseClassNameLabel;

    [Export] private Button _addRowButton;
    
    #endregion

    #region [Main]

    public void StartUp()
    {
        Instance = this;
        _sheetsItemTree.ItemActivated += HandleSheetsItemSelected;
        _addRowButton.Pressed += HandleClickAddRowButton;
        GetWindow().FocusExited += HandleWindowFocusExited;
        SikaSheetPlugin.Instance.GetEditorInterface().GetResourceFilesystem().FilesystemChanged += HandleFileSystemChanged;
        InitMainMenuButton();   
    }

    public void ShutDown()
    {
        GetWindow().FocusExited -= HandleWindowFocusExited;
        SikaSheetPlugin.Instance.GetEditorInterface().GetResourceFilesystem().FilesystemChanged -= HandleFileSystemChanged;
    }
  
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        // auto dispose 
        using var _ = @event;

        UpdateFocusOnInput(@event);

        // receive input
        if(!IsFocused)
            return;
        
        if (@event.IsActionPressed("ui_undo") && !@event.IsEcho())
        {
            if (SheetReader.CanUndo)
                SheetReader.UndoHistory();
            
            // accept undo/redo event while focused, avoid unexpected global undo/redo
            AcceptEvent();
        } 
        else if (@event.IsActionPressed("ui_redo") && !@event.IsEcho())
        {
            if (SheetReader.CanRedo)
                SheetReader.RedoHistory();
            
            // accept undo/redo event while focused, avoid unexpected global undo/redo
            AcceptEvent();
        }
    }
 
    public override void _Draw()
    {
        base._Draw();
        DrawFocus();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_fileSystemDirtyFlag)
        {
            _fileSystemDirtyFlag = false;
            SheetReader?.FixSheetResource();
        }
    }

    public void ShowEditor()
    {
        Visible = true;
        SetAnchorsPreset(LayoutPreset.FullRect);
        InitSheetsTree();
        RefreshEditorSheetVisibility();
        _sheetView.SetDirty();
    }

    public void HideEditor()
    {
        Visible = false;
    }

    private void RefreshEditorSheetVisibility()
    {
        _hasSheetRoot.Visible = SheetReader != null;
        _emptySheetRoot.Visible = SheetReader == null;
    }

    private void HandleFileSystemChanged()
    {
        _fileSystemDirtyFlag = true;
    }

    #endregion

    #region [Focus]

    public bool IsFocused { get; private set; }

    private void SetFocus(bool isFocused)
    {
        if(isFocused == IsFocused)
            return;

        IsFocused = isFocused;
        QueueRedraw();
        CancelFocusedChildControl();
    }

    private void UpdateFocusOnInput(InputEvent @event)
    {
        if (!GetWindow().HasFocus())
            return;

        // update focus 
        if (IsVisibleInTree() &&
            @event is InputEventMouseButton mouseButton &&
            mouseButton.Pressed &&
            !mouseButton.IsEcho() &&
            mouseButton.ButtonIndex == MouseButton.Left)
        {
            var isFocused = GetGlobalRect().HasPoint(mouseButton.GlobalPosition);
            SetFocus(isFocused);
        }
    }
    
    private void CancelFocusedChildControl()
    {
        var stack = new Stack<Control>();
        stack.Push(this);
        while (stack.Count > 0)
        {
            var control = stack.Pop();
            if(!control.Visible)
                continue;

            if (control.HasFocus())
            {
                control.ReleaseFocus();
                return;
            }

            var controlChildren = control.GetChildren().Where(c => c is Control { Visible: true }).OfType<Control>().ToList();
            foreach (var controlChild in controlChildren)
                stack.Push(controlChild);
        }
    }

    private void DrawFocus()
    {
        if (!IsFocused)
            return;
        
        var rect = GetRect();
        rect = rect.Grow(-1);
        DrawPolyline(
            new[]
            {
                rect.Position,
                new Vector2(rect.Position.X, rect.End.Y),
                rect.End,
                new Vector2(rect.End.X, rect.Position.Y),
                rect.Position,
            },
            Colors.LightSkyBlue, 2f);
    }

    private void HandleWindowFocusExited()
    {
        SheetLogger.LogDebug("Window focus exited!");
        CancelFocusedChildControl();
    }
    
    #endregion
    
    #region [Menu]

    private const int UndoId = 1;
    private const int RedoId = 2;
    private const int EnableQuickDeleteId = 3;
    private const int GenerateCodeId = 4;
    
    
    private void InitMainMenuButton()
    {
        if(_mainMenuButton == null)
            return;

        var popup = _mainMenuButton.GetPopup();
        popup.Clear();
        popup.AddItem("Undo", UndoId, (Key)((long)KeyModifierMask.MaskCmdOrCtrl | (long)Key.Z));
        popup.AddItem("Redo", RedoId, (Key)((long)KeyModifierMask.MaskCmdOrCtrl | (long)Key.Y));
        popup.AddItem("Enable quick delete", EnableQuickDeleteId);
        popup.AddSeparator();
        popup.AddItem("Generate code for SheetReaders", GenerateCodeId);
        
        popup.IdPressed += HandleMainMenuIdPressed;
        _mainMenuButton.Pressed += HandleMainMenuButtonPressed;
    }

    private void HandleMainMenuButtonPressed()
    { 
        var popup = _mainMenuButton.GetPopup();
        popup.SetItemDisabled(0, !(SheetReader?.CanUndo ?? false));
        popup.SetItemDisabled(1, !(SheetReader?.CanRedo ?? false));
        
        var enableQuickDeleteIcon = SheetConst.IconPath + (EnableQuickDeleteRow ? "/radio_on.svg" : "/radio_off.svg");
        popup.SetItemIcon(0, ResourceLoader.Load<Texture2D>(SheetConst.IconPath + "/undo.svg"));
        popup.SetItemIcon(1, ResourceLoader.Load<Texture2D>(SheetConst.IconPath + "/undo.svg"));
        popup.SetItemIcon(2, ResourceLoader.Load<Texture2D>(enableQuickDeleteIcon));
        popup.SetItemIcon(4, ResourceLoader.Load<Texture2D>(SheetConst.IconPath + "/generate_code.svg"));
    }

    private void HandleMainMenuIdPressed(long id)
    {
        switch (id)
        {
            case UndoId:
                if (SheetReader?.CanUndo ?? false)
                    SheetReader.UndoHistory();
                break;
            case RedoId:
                if (SheetReader?.CanRedo ?? false)
                    SheetReader.RedoHistory();
                break;
            case EnableQuickDeleteId:
                EnableQuickDeleteRow = !EnableQuickDeleteRow;
                break;
            case GenerateCodeId:
                SheetCodeGenerateUtility.GenerateCode();
                break;
            default:
                throw new NotImplementedException();
        }
    }

    #endregion
    
    #region [Sheets Tree]

    private void InitSheetsTree()
    {
        var sheetTypes = SheetDatabase.GetSheetDataTypes();
        SheetLogger.Log($"SheetsCount : {sheetTypes.Count}");
        
        _sheetsItemTree.Clear();
        var root = _sheetsItemTree.CreateItem();
        _sheetsItemTree.HideRoot = true;

        var groupDict = new Dictionary<string, TreeItem>();
        var groupIcon = ResourceLoader.Load<Texture2D>(SheetConst.IconPath + "/folder.svg");
        foreach (var sheetType in sheetTypes)
        {
            var groupAttribute = sheetType.GetCustomAttribute<SheetGroupAttribute>();
            var groupName = groupAttribute?.GroupName ?? "Default";
            if (!groupDict.TryGetValue(groupName, out var groupNode))
            {
                groupNode = _sheetsItemTree.CreateItem(root);
                groupNode.SetText(0, groupName);
                groupNode.SetIcon(0, groupIcon);
                groupNode.SetMetadata(0, true);
                groupDict.Add(groupName, groupNode);
            }

            TreeItem sheetItem = _sheetsItemTree.CreateItem(groupNode);
            sheetItem.SetText(0, sheetType.Name);
        }
    }

    private void HandleSheetsItemSelected()
    {
        var treeItem = _sheetsItemTree.GetSelected();
        // is group
        if(treeItem.GetMetadata(0).AsBool())
            return;
        
        var sheetName = treeItem.GetText(0);
        SheetLogger.LogDebug("double click select " + sheetName);
        InitSheet(sheetName);
    }

    private void HandleClickGenerateCodeButton()
    {
        SheetCodeGenerateUtility.GenerateCode();
    }

    #endregion

    #region [Selected Sheet]

    public void InitSheet(string sheetName)
    {
        SheetReader = null;
        CurrentSheetName = null;
        if(!SheetDatabase.TryReadSheetText(sheetName, true, out var jsonText, out var sheetType))
            return;

        CurrentSheetName = sheetName;
        SelectSheetInTree(sheetName);
        
        SheetReader = new EditorSheetReader(sheetType);
        SheetReader.OnSheetDataChange += HandleSheetDataChange;
        SheetReader.OnHistoryChange += HandleHistoryChange;
        SheetReader.ReadSheet();
        SheetLogger.LogDebug($"InitSheet : {SheetReader.SheetDataType.Name}");
        
        UpdateSheetTitle();
        _sheetView.Show(SheetReader);
        RefreshEditorSheetVisibility();
        
        HandleSheetDataChange();
        HandleHistoryChange();
    }

    private void HandleSheetDataChange()
    {
        _sheetView.SetDirty();
    }

    private void HandleHistoryChange()
    {
        SheetReader.GetHistoryCount(out var prevCount, out var nextCount);
        _historyLabel.Text = $"History : undo_{prevCount}, redo_{nextCount}";
    }

    private void SelectSheetInTree(string sheetName)
    {
        var root = _sheetsItemTree.GetRoot();
        if(root == null)
            return;
        
        foreach (var groupTreeItem in root.GetChildren())
        {
            foreach (var sheetTreeItem in groupTreeItem.GetChildren())
            {
                if (sheetTreeItem.GetText(0) == sheetName)
                {
                    _sheetsItemTree.SetSelected(sheetTreeItem, 0);
                    return;
                }
            } 
        }
    }
  
    private void UpdateSheetTitle()
    {
        var dataType = SheetReader.SheetDataType;
        _sheetClassNameLabel.Text = dataType.Name;

        var sb = new StringBuilder();
        while (dataType != null)
        {
            dataType = dataType.BaseType;
            if (dataType != null)
            {
                sb.Append(" -> ");
                sb.Append(dataType.Name);
            }
            
            if(dataType == typeof(SheetData))
                break;
        }
        _sheetBaseClassNameLabel.Text = sb.ToString();
    }

    private void HandleClickAddRowButton()
    {
        SheetReader?.AddRow();
    }
    
    #endregion
}

#endif
