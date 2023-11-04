# SikaSheet
## Workflow
- Define CustomSheetData, for example
```csharp
public class ExampleSheetData : SheetData
{
	// use [SheetKey(0)] to define the first key
	[SheetKey(0)]
	public string ID;
	
	// Public field member will be serialized
	public int HP;
	
	// Public get/set property member will be serialized
	public int PropertyMember { get; set; }
	
	// !!! Private or protected member will not be serialized
	private int _privateMember;
}
```

- Build C# project
- Edit data via SikaSheet Editor
- Generate SheetReader code and Build C# project : `SikaSheetEditor - Menu - GenerateCodeForSheetReaders`
- Read data via SheetReader
```cs
var data = ExampleDataSheetReader.Get(id1, id2);
```

## Usages of Paths
All paths are defined in `SheetConst.cs`.
- `user_data/DataDefines` : Define custom sheet data classes
- `user_data/Sheets` : Store all sheets with json format  
- `user_data/Generated` : Generate SheetReader codes here  
