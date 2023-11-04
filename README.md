# SikaSheet  
This is a data sheet plugin for Godot4 **C#** project.  
Define Data & Edit Data & Read Data All in one.  
![image](https://github.com/Jamesika/SikaSheet/assets/23502082/c8b0c98d-d92d-4bc7-a120-1e1594f138fb)


## Main Features
- Generate sheet struct by custom defined C# class
- Support many types (And can be extended):
	- Primitive data types
 		- bool
		- int/uint
  		- float/double 
		- string
 	- Enum Types
  		- Support Attribute `[Flags]`  
    		![image](https://github.com/Jamesika/SikaSheet/assets/23502082/516408e3-fe2d-4807-9502-c52d05ce21a6)
  	- GodotEngine Types
  		- Vector2
  	 	- Vector2I
  	  	- Color
  	- GodotEngine Resource Types
  		- it's wrapped by struct `ResourceRef<T>`, so we need to define a member like this `ResourceRef<Texture2D>`
  	   	![image](https://github.com/Jamesika/SikaSheet/assets/23502082/098f25ba-d758-4b02-ad14-6b27ba21b81b)
	- C# Array
 	- Custom data type : must derived from `SheetNestedData`
- Undo/Redo
- Group sheets using Attribute `[SheetGroup("GroupName")]`  
![image](https://github.com/Jamesika/SikaSheet/assets/23502082/06d5a663-fbfa-4a4a-8630-c4cd1c078c91)
- Generate sheet reader data for a sheet.
- Define keys for a sheet using `[SheetKey(keyIndex)]`, so you can read any data by keys, can define multiple keys.
- Support sheet data type inheritance, SheetReader of superclass can read all data from Sheets of subclasses.

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

## How to install this plugin
Copy folder `addons/SikaSheet` to your projects at the same path.
If you need examples, copy folder `SikaSheetUserData`, too.
Build C# project, enable the plugin, have a try!

## Apology
I'm a refugee from UnityEngine since Sep 2023, so I tried to use GodotEngine to make something, I made this plugin and some game demo :).
But finally I have to back to Unity to continue my work. It means I don't have too much time to maintain or add new features for this plugin project.
