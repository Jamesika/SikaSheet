// This file is automaticlly generated
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace SikaSheet
{
	public  class ExampleSheetData3SheetReader : RuntimeSheetReader<ExampleSheetData3>
	{
		private static ExampleSheetData3SheetReader _instance;
	
		private static ExampleSheetData3SheetReader Instance {
			get
			{
				if (_instance != null)
					_instance.EnsureSheetDataNotDirty();
				else
					_instance = CreateInstance();	

				return _instance;
			}
		}

		private Dictionary<System.String, ExampleSheetData3> _keyToData = new Dictionary<System.String, ExampleSheetData3>();

		/// <summary>
		/// include self data list and data list from all subclasses
		/// </summary>
		private List<ExampleSheetData3> _allSheetDataList;

		private static ExampleSheetData3SheetReader CreateInstance()
		{
			var instance = new ExampleSheetData3SheetReader(typeof(ExampleSheetData3));
			instance.ReadSheet();
			return instance;
		}

		public ExampleSheetData3SheetReader(Type sheetDataType) : base(sheetDataType)
		{
		}

		private static bool IsKeyEquals(ExampleSheetData3 data, System.String ID)
		{
			return data.ID == ID;
		}

		private ExampleSheetData3 GetImpl(System.String ID)
		{
			var key = ID;
			if (Equals(key, null))
			{   
				SheetLogger.LogError("ExampleSheetData3 try to get data with null Key!");
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

		public static ExampleSheetData3 Get(System.String ID)
		{
			var data = Instance.GetImpl(ID);
			if (data == null)
				SheetLogger.LogError($"ExampleSheetData3 get data failed, key : {ID}");

			return data;
		}

		private List<ExampleSheetData3> GetAllSheetDataImpl()
		{
			if (_allSheetDataList != null && _allSheetDataList.Count > 0)
				return _allSheetDataList;

			_allSheetDataList = new List<ExampleSheetData3>();
			if(Instance != null)
				_allSheetDataList.AddRange(Instance.RowDataListT);



			return _allSheetDataList;
		}

		public static List<ExampleSheetData3> GetAll()
		{
			return Instance.GetAllSheetDataImpl();
		}

		public static bool TryGet(System.String ID, out ExampleSheetData3 data)
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
