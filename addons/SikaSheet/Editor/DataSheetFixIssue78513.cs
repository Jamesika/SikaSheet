#if TOOLS 

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.Json;

namespace SikaSheet;

public static class DataSheetFixIssue78513
{
    // To avoid this issue : https://github.com/godotengine/godot/issues/78513
    [ModuleInitializer]
    public static void Initialize()
    {
        AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly()).Unloading += alc =>
        {  
            var assembly = typeof(JsonSerializerOptions).Assembly;
            var updateHandlerType = assembly.GetType("System.Text.Json.JsonSerializerOptionsUpdateHandler");
            var clearCacheMethod = updateHandlerType?.GetMethod("ClearCache", BindingFlags.Static | BindingFlags.Public);
            clearCacheMethod?.Invoke(null, new object?[] { null });

            // Unload any other unloadable references
        };
    }
}
  
#endif