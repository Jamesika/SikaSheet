#if TOOLS

using System.Linq;
using Godot;
namespace SikaSheet;

[SheetValueDefine(typeof(IResourceRef))]
public partial class ResourceRefProperty : SheetValueProperty
{
    private TextureRect _textureRect;
    private Label _label;
    private TextureButton _selectFileButton;
    private TextureButton _deleteButton;

    public override void _Ready()
    {
        base._Ready();

        _textureRect = new TextureRect();
        _label = new Label();
        _selectFileButton = new TextureButton();
        _deleteButton = new TextureButton();

        _textureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        _textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        _textureRect.CustomMinimumSize = new Vector2(30, 30);
        
        _selectFileButton.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
        _selectFileButton.TextureNormal = ResourceLoader.Load<Texture2D>(SheetConst.IconPath + "/folder.svg");
        
        _deleteButton.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
        _deleteButton.TextureNormal = ResourceLoader.Load<Texture2D>(SheetConst.IconPath + "/delete.png");
        _deleteButton.TexturePressed = _deleteButton.TextureHover = ResourceLoader.Load<Texture2D>(SheetConst.IconPath + "/delete_pressed.png");
        
        AddChild(_textureRect);
        AddChildExpandAndFill(_label);
        AddChild(_selectFileButton);
        AddChild(_deleteButton);

        _selectFileButton.Pressed += HandleClickSelectFileButton;
        _deleteButton.Pressed += HandleClickDeleteButton;
    }

    public override void _GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && !mouseButton.IsEcho() &&
            mouseButton.ButtonIndex == MouseButton.Left)
        {
            // select in file system
            var resourceRef = GetValue<IResourceRef>();
            if (ResourceLoader.Exists(resourceRef.Path))
                SikaSheetPlugin.Instance.GetEditorInterface().SelectFile(resourceRef.Path);
        }
    }

    protected override void ShowRawValue()
    {
        var resourceRef = GetValue<IResourceRef>();
        _label.Text = GetResourcePathForView(resourceRef);
        _textureRect.Texture = null;
        if (!string.IsNullOrEmpty(resourceRef.Path))
        {
            if(resourceRef.RawValue == null)
                return;
            
            var resourcePreviewer = SikaSheetPlugin.Instance.GetEditorInterface().GetResourcePreviewer();
            resourcePreviewer.QueueResourcePreview(resourceRef.Path, this, nameof(ReceivePreviewTextureCallback), default);
        }
    }

    private string GetResourcePathForView(IResourceRef resourceRef)
    {
        var path = resourceRef.Path ?? string.Empty;
        if (string.IsNullOrEmpty(path))
            return path;

        var pathSplits = path.Replace("res://", "").Split('/');
        if (pathSplits.Length > 2)
            return $".../{pathSplits[^2]}/{pathSplits[^1]}";

        return path;
    }

    private void ReceivePreviewTextureCallback(string path, Texture2D preview, Texture2D thumbnailPreview, Variant userData)
    {
        SheetLogger.LogDebug("Success preview texture : " + path);
        _textureRect.Texture = preview ?? thumbnailPreview;
    }

    private void TryUpdateResource(Resource resource, bool showTypeNotMatchDialog = false)
    {
        var resourceRef = GetValue<IResourceRef>();
        if (resourceRef.IsAssignable(resource))
        {
            resourceRef.UpdateResource(resource);
            SetValue(resourceRef);
            HandleValueChanged();
            return;
        }

        var errorText = "Cant set resource to sheet cell : " + resource.ResourcePath;
        if(showTypeNotMatchDialog)
        {
            AcceptDialog acceptDialog = new AcceptDialog();
            acceptDialog.InitialPosition = Window.WindowInitialPosition.CenterMainWindowScreen;
            acceptDialog.DialogText = errorText;
            AddChild(acceptDialog);
            acceptDialog.Show();
        }

        SheetLogger.LogError(errorText);
    }

    #region [Drag]

    public override Variant _GetDragData(Vector2 atPosition)
    {
        var resource = GetValue<IResourceRef>().RawValue;
        if (resource != null)
        {
            var pathLabel = new Label();
            pathLabel.Text = _label.Text;
            SetDragPreview(pathLabel);
            return resource;
        }

        return default;
    }
    
    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        var resource = GetResourceFromDropData(data);
        return resource != null && resource.GetType().IsAssignableTo(GetValue<IResourceRef>().RequiredResourceType);
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        base._DropData(atPosition, data);
        var resource = GetResourceFromDropData(data);
        if (resource != null)
            TryUpdateResource(resource);
    }

    private Resource GetResourceFromDropData(Variant data)
    {
        if (data.Obj is Resource resource && !string.IsNullOrEmpty(resource.ResourcePath))
            return resource;

        if (data.VariantType == Variant.Type.Dictionary && data.AsGodotDictionary().TryGetValue("files", out var files))
        {
            var stringArray = files.AsStringArray();
            if (stringArray.Length > 0)
            {
                var filePath = stringArray[0];
                return ResourceLoader.Load(filePath);
            }
        }

        return null;
    }
    
    #endregion

    #region [FileSelection]

    private void HandleClickSelectFileButton()
    {
        EditorFileDialog fileDialog = new EditorFileDialog();
        fileDialog.Access = EditorFileDialog.AccessEnum.Resources;
        fileDialog.DisplayMode = EditorFileDialog.DisplayModeEnum.Thumbnails;
        fileDialog.FileMode = EditorFileDialog.FileModeEnum.OpenFile;
        fileDialog.InitialPosition = Window.WindowInitialPosition.CenterMainWindowScreen;
        fileDialog.Size = new Vector2I(1050, 700);
        SetEditorFileDialogFilters(fileDialog);
        
        fileDialog.FileSelected += HandleFileSelected;
        
        AddChild(fileDialog);
        fileDialog.Popup();
    }

    private void HandleClickDeleteButton()
    {
        TryUpdateResource(null);
    }

    private void SetEditorFileDialogFilters(EditorFileDialog fileDialog)
    {
        var baseType = GetValue<IResourceRef>().RequiredResourceType;
        var fileExtensions = ResourceLoader.GetRecognizedExtensionsForType(baseType.Name).ToList();
        fileDialog.ClearFilters();
        foreach (var extension in fileExtensions)
            fileDialog.AddFilter($"*.{extension}", extension.ToUpper());
    }

    private void HandleFileSelected(string filePath)
    {
        var resource = ResourceLoader.Load<Resource>(filePath);
        if(resource != null)
            TryUpdateResource(resource, true);
    }

    #endregion
}

#endif