using Godot;

namespace SikaSheet;

public static class SheetLogger
{
    //private static bool _enableDebug = true;
    private static bool _enableDebug = false;
    
    public static void Log(string info)
    {
        if (_enableDebug)
            GD.Print($"[{Engine.GetProcessFrames()}] [SikaSheet] " + info);
        else
            GD.Print($"[SikaSheet] " + info);
    }

    public static void LogError(string info)
    {
        if(_enableDebug)
            GD.PrintErr($"[{Engine.GetProcessFrames()}] [SikaSheet] " + info);
        else 
            GD.PrintErr($"[SikaSheet] " + info);
    }

    public static void LogDebug(string info)
    {
        if (_enableDebug)
            GD.Print($"[Debug] [{Engine.GetProcessFrames()}]" + info);
    }
}