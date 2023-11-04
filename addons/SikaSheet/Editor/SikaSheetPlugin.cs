#if TOOLS

using Godot;

namespace SikaSheet;

[Tool]
public partial class SikaSheetPlugin : EditorPlugin, ISerializationListener
{
	private PackedScene _dataSheetEditorPackedScene = ResourceLoader.Load<PackedScene>(SheetConst.MainScenePath);

	private DataSheetEditor _sheetEditor;

	public static SikaSheetPlugin Instance { get; private set; }

	public SikaSheetPlugin()
	{
		// static member cant be deserialized after Build
		Instance = this;
	}
	
	public override void _EnterTree()
	{
		base._EnterTree();
		CreateSheet();
	}
  
	public override void _ExitTree()
	{
		base._ExitTree();
		ClearSheet();
	}

	public override bool _HasMainScreen()
	{
		return true;
	}

	public override void _MakeVisible(bool visible)
	{
		if(visible)
			_sheetEditor.ShowEditor(); 
		else 
			_sheetEditor.HideEditor();
	}

	public override string _GetPluginName()
	{
		return "SikaSheet";
	}

	public override Texture2D _GetPluginIcon()
	{
		return ResourceLoader.Load<Texture2D>(SheetConst.IconPath + "/table.svg");
	}

	#region [Sheet]
  
	private void CreateSheet()
	{  
		ClearSheet();

		SheetLogger.LogDebug("DataSheetPlugin CreateSheet");
		_sheetEditor = (DataSheetEditor)_dataSheetEditorPackedScene.Instantiate();
		GetEditorInterface().GetEditorMainScreen().AddChild(_sheetEditor);
		_sheetEditor.StartUp();
		_sheetEditor.HideEditor();
	}

	private void ClearSheet()
	{
		if (IsInstanceValid(_sheetEditor))
		{
			SheetLogger.LogDebug("DataSheetPlugin ClearSheet");
			_sheetEditor.ShutDown();
			_sheetEditor.QueueFree();   
		}
   
		_sheetEditor = null;
	}

	#endregion

	#region [ISerializationListener]
	
	private string _currentSheetName;
	private bool _isSheetVisible;
	
	void ISerializationListener.OnBeforeSerialize()
	{
		if (_sheetEditor != null)
		{
			_currentSheetName = _sheetEditor.CurrentSheetName;
			_isSheetVisible = _sheetEditor.Visible;
		}
		else
		{
			_currentSheetName = null;
			_isSheetVisible = false;
		}

		ClearSheet();
	}

	void ISerializationListener.OnAfterDeserialize()
	{
		CreateSheet();
		if(_isSheetVisible)
			_sheetEditor.ShowEditor();
		
		if (!string.IsNullOrEmpty(_currentSheetName))
			_sheetEditor.InitSheet(_currentSheetName);
	}
	
	#endregion
}
#endif
