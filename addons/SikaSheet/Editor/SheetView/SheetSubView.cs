#if TOOLS

using Godot;

namespace SikaSheet;

[Tool]
public abstract partial class SheetSubView : Control
{
    protected SheetView MainView { get; private set; }

    public abstract bool GetMinRowSize(int index, out float size);
    public abstract bool GetMinColumnSize(int index, out float size);

    public void Initialize(SheetView sheetView)
    {
        MainView = sheetView;
    }

    public abstract void PreRebuild();
    
    public abstract void Rebuild();

    public abstract void ClearCache();
}

#endif 