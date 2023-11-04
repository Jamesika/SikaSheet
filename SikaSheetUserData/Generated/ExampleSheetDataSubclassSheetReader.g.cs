// This file is automaticlly generated
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace SikaSheet
{
	public  class ExampleSheetDataSubclassSheetReader : RuntimeSheetReader<ExampleSheetDataSubclass>
	{
		private static ExampleSheetDataSubclassSheetReader _instance;
	
		private static ExampleSheetDataSubclassSheetReader Instance {
			get
			{
				if (_instance != null)
					_instance.EnsureSheetDataNotDirty();
				else
					_instance = CreateInstance();	

				return _instance;
			}
		}

		private Dictionary<System.String, ExampleSheetDataSubclass> _keyToData = new Dictionary<System.String, ExampleSheetDataSubclass>();

		/// <summary>
		/// include self data list and data list from all subclasses
		/// </summary>
		private List<ExampleSheetDataSubclass> _allSheetDataList;

		private static ExampleSheetDataSubclassSheetReader CreateInstance()
		{
			var instance = new ExampleSheetDataSubclassSheetReader(typeof(ExampleSheetDataSubclass));
			instance.ReadSheet();
			return instance;
		}

		public ExampleSheetDataSubclassSheetReader(Type sheetDataType) : base(sheetDataType)
		{
		}

		private static bool IsKeyEquals(ExampleSheetDataSubclass data, System.String ID)
		{
			return data.ID == ID;
		}

		private ExampleSheetDataSubclass GetImpl(System.String ID)
		{
			var key = ID;
			if (Equals(key, null))
			{   
				SheetLogger.LogError("ExampleSheetDataSubclass try to get data with null Key!");
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

		public static ExampleSheetDataSubclass Get(System.String ID)
		{
			var data = Instance.GetImpl(ID);
			if (data == null)
				SheetLogger.LogError($"ExampleSheetDataSubclass get data failed, key : {ID}");

			return data;
		}

		private List<ExampleSheetDataSubclass> GetAllSheetDataImpl()
		{
			if (_allSheetDataList != null && _allSheetDataList.Count > 0)
				return _allSheetDataList;

			_allSheetDataList = new List<ExampleSheetDataSubclass>();
			if(Instance != null)
				_allSheetDataList.AddRange(Instance.RowDataListT);



			return _allSheetDataList;
		}

		public static List<ExampleSheetDataSubclass> GetAll()
		{
			return Instance.GetAllSheetDataImpl();
		}

		public static bool TryGet(System.String ID, out ExampleSheetDataSubclass data)
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
