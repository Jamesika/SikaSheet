// This file is automaticlly generated
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace SikaSheet
{
	public  class ExampleSheetDataSheetReader : RuntimeSheetReader<ExampleSheetData>
	{
		private static ExampleSheetDataSheetReader _instance;
	
		private static ExampleSheetDataSheetReader Instance {
			get
			{
				if (_instance != null)
					_instance.EnsureSheetDataNotDirty();
				else
					_instance = CreateInstance();	

				return _instance;
			}
		}

		private Dictionary<(System.String, System.String), ExampleSheetData> _keyToData = new Dictionary<(System.String, System.String), ExampleSheetData>();

		/// <summary>
		/// include self data list and data list from all subclasses
		/// </summary>
		private List<ExampleSheetData> _allSheetDataList;

		private static ExampleSheetDataSheetReader CreateInstance()
		{
			var instance = new ExampleSheetDataSheetReader(typeof(ExampleSheetData));
			instance.ReadSheet();
			return instance;
		}

		public ExampleSheetDataSheetReader(Type sheetDataType) : base(sheetDataType)
		{
		}

		private static bool IsKeyEquals(ExampleSheetData data, System.String ID2, System.String ID)
		{
			return (data.ID2 == ID2&&  data.ID == ID);
		}

		private ExampleSheetData GetImpl(System.String ID2, System.String ID)
		{
			var key = (ID2, ID);
			if (Equals(key, null))
			{   
				SheetLogger.LogError("ExampleSheetData try to get data with null Key!");
				return null;
			}

			if (_keyToData.TryGetValue(key, out var value))
				return value;

			var allSheetDataList = GetAllSheetDataImpl();
			foreach (var sheetData in allSheetDataList)
			{
				if (IsKeyEquals(sheetData, ID2, ID))
				{
					_keyToData.Add(key, sheetData);
					return sheetData;
				}
			}

			return null;
		}

		public static ExampleSheetData Get(System.String ID2, System.String ID)
		{
			var data = Instance.GetImpl(ID2, ID);
			if (data == null)
				SheetLogger.LogError($"ExampleSheetData get data failed, key : ({ID2}, {ID})");

			return data;
		}

		private List<ExampleSheetData> GetAllSheetDataImpl()
		{
			if (_allSheetDataList != null && _allSheetDataList.Count > 0)
				return _allSheetDataList;

			_allSheetDataList = new List<ExampleSheetData>();
			if(Instance != null)
				_allSheetDataList.AddRange(Instance.RowDataListT);



			return _allSheetDataList;
		}

		public static List<ExampleSheetData> GetAll()
		{
			return Instance.GetAllSheetDataImpl();
		}

		public static bool TryGet(System.String ID2, System.String ID, out ExampleSheetData data)
		{
			data = Instance.GetImpl(ID2, ID);
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
