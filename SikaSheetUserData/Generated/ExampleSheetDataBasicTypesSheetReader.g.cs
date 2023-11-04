// This file is automaticlly generated
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace SikaSheet
{
	public  class ExampleSheetDataBasicTypesSheetReader : RuntimeSheetReader<ExampleSheetDataBasicTypes>
	{
		private static ExampleSheetDataBasicTypesSheetReader _instance;
	
		private static ExampleSheetDataBasicTypesSheetReader Instance {
			get
			{
				if (_instance != null)
					_instance.EnsureSheetDataNotDirty();
				else
					_instance = CreateInstance();	

				return _instance;
			}
		}

		private Dictionary<System.String, ExampleSheetDataBasicTypes> _keyToData = new Dictionary<System.String, ExampleSheetDataBasicTypes>();

		/// <summary>
		/// include self data list and data list from all subclasses
		/// </summary>
		private List<ExampleSheetDataBasicTypes> _allSheetDataList;

		private static ExampleSheetDataBasicTypesSheetReader CreateInstance()
		{
			var instance = new ExampleSheetDataBasicTypesSheetReader(typeof(ExampleSheetDataBasicTypes));
			instance.ReadSheet();
			return instance;
		}

		public ExampleSheetDataBasicTypesSheetReader(Type sheetDataType) : base(sheetDataType)
		{
		}

		private static bool IsKeyEquals(ExampleSheetDataBasicTypes data, System.String ID)
		{
			return data.ID == ID;
		}

		private ExampleSheetDataBasicTypes GetImpl(System.String ID)
		{
			var key = ID;
			if (Equals(key, null))
			{   
				SheetLogger.LogError("ExampleSheetDataBasicTypes try to get data with null Key!");
				return null;
			}

			if (_keyToData.TryGetValue(key, out var value))
				return value;

			var allSheetDataList = GetAllSheetDataImpl();
			foreach (var sheetData in allSheetDataList)
			{
				if (IsKeyEquals(sheetData, ID))
				{
					_keyToData.Add(key, sheetData);
					return sheetData;
				}
			}

			return null;
		}

		public static ExampleSheetDataBasicTypes Get(System.String ID)
		{
			var data = Instance.GetImpl(ID);
			if (data == null)
				SheetLogger.LogError($"ExampleSheetDataBasicTypes get data failed, key : {ID}");

			return data;
		}

		private List<ExampleSheetDataBasicTypes> GetAllSheetDataImpl()
		{
			if (_allSheetDataList != null && _allSheetDataList.Count > 0)
				return _allSheetDataList;

			_allSheetDataList = new List<ExampleSheetDataBasicTypes>();
			if(Instance != null)
				_allSheetDataList.AddRange(Instance.RowDataListT);



			return _allSheetDataList;
		}

		public static List<ExampleSheetDataBasicTypes> GetAll()
		{
			return Instance.GetAllSheetDataImpl();
		}

		public static bool TryGet(System.String ID, out ExampleSheetDataBasicTypes data)
		{
			data = Instance.GetImpl(ID);
			return data != null;
		}

		protected override void ClearCache()
		{
			base.ClearCache();
			_allSheetDataList = null;
			_keyToData.Clear();
		}
	}
}
