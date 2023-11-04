// This file is automaticlly generated
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace SikaSheet
{
	public  class ExampleSheetDataBaseSheetReader : RuntimeSheetReader<ExampleSheetDataBase>
	{
		private static ExampleSheetDataBaseSheetReader _instance;
	
		private static ExampleSheetDataBaseSheetReader Instance {
			get
			{
				if (_instance != null)
					_instance.EnsureSheetDataNotDirty();
				else
					_instance = CreateInstance();	

				return _instance;
			}
		}

		private Dictionary<System.String, ExampleSheetDataBase> _keyToData = new Dictionary<System.String, ExampleSheetDataBase>();

		/// <summary>
		/// include self data list and data list from all subclasses
		/// </summary>
		private List<ExampleSheetDataBase> _allSheetDataList;

		private static ExampleSheetDataBaseSheetReader CreateInstance()
		{
			var instance = new ExampleSheetDataBaseSheetReader(typeof(ExampleSheetDataBase));
			instance.ReadSheet();
			return instance;
		}

		public ExampleSheetDataBaseSheetReader(Type sheetDataType) : base(sheetDataType)
		{
		}

		private static bool IsKeyEquals(ExampleSheetDataBase data, System.String ID)
		{
			return data.ID == ID;
		}

		private ExampleSheetDataBase GetImpl(System.String ID)
		{
			var key = ID;
			if (Equals(key, null))
			{   
				SheetLogger.LogError("ExampleSheetDataBase try to get data with null Key!");
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

		public static ExampleSheetDataBase Get(System.String ID)
		{
			var data = Instance.GetImpl(ID);
			if (data == null)
				SheetLogger.LogError($"ExampleSheetDataBase get data failed, key : {ID}");

			return data;
		}

		private List<ExampleSheetDataBase> GetAllSheetDataImpl()
		{
			if (_allSheetDataList != null && _allSheetDataList.Count > 0)
				return _allSheetDataList;

			_allSheetDataList = new List<ExampleSheetDataBase>();
			if(Instance != null)
				_allSheetDataList.AddRange(Instance.RowDataListT);

            _allSheetDataList.AddRange(ExampleSheetDataSubclassSheetReader.GetAll());


			return _allSheetDataList;
		}

		public static List<ExampleSheetDataBase> GetAll()
		{
			return Instance.GetAllSheetDataImpl();
		}

		public static bool TryGet(System.String ID, out ExampleSheetDataBase data)
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
