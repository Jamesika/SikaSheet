// This file is automaticlly generated
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace SikaSheet
{
	public  class ExampleSheetData2SheetReader : RuntimeSheetReader<ExampleSheetData2>
	{
		private static ExampleSheetData2SheetReader _instance;
	
		private static ExampleSheetData2SheetReader Instance {
			get
			{
				if (_instance != null)
					_instance.EnsureSheetDataNotDirty();
				else
					_instance = CreateInstance();	

				return _instance;
			}
		}

		private Dictionary<System.String, ExampleSheetData2> _keyToData = new Dictionary<System.String, ExampleSheetData2>();

		/// <summary>
		/// include self data list and data list from all subclasses
		/// </summary>
		private List<ExampleSheetData2> _allSheetDataList;

		private static ExampleSheetData2SheetReader CreateInstance()
		{
			var instance = new ExampleSheetData2SheetReader(typeof(ExampleSheetData2));
			instance.ReadSheet();
			return instance;
		}

		public ExampleSheetData2SheetReader(Type sheetDataType) : base(sheetDataType)
		{
		}

		private static bool IsKeyEquals(ExampleSheetData2 data, System.String ID)
		{
			return data.ID == ID;
		}

		private ExampleSheetData2 GetImpl(System.String ID)
		{
			var key = ID;
			if (Equals(key, null))
			{   
				SheetLogger.LogError("ExampleSheetData2 try to get data with null Key!");
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

		public static ExampleSheetData2 Get(System.String ID)
		{
			var data = Instance.GetImpl(ID);
			if (data == null)
				SheetLogger.LogError($"ExampleSheetData2 get data failed, key : {ID}");

			return data;
		}

		private List<ExampleSheetData2> GetAllSheetDataImpl()
		{
			if (_allSheetDataList != null && _allSheetDataList.Count > 0)
				return _allSheetDataList;

			_allSheetDataList = new List<ExampleSheetData2>();
			if(Instance != null)
				_allSheetDataList.AddRange(Instance.RowDataListT);



			return _allSheetDataList;
		}

		public static List<ExampleSheetData2> GetAll()
		{
			return Instance.GetAllSheetDataImpl();
		}

		public static bool TryGet(System.String ID, out ExampleSheetData2 data)
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
