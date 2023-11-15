using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class UnitHelper
	{
		private static UnitHelper _instance;

		private readonly Dictionary<int, int> _oldUnitIdToNewUnitIdMap;

		private Func<int, IUnit> _unitGetter;

		public UnitHelper()
		{
			_instance = this;
			_oldUnitIdToNewUnitIdMap = new Dictionary<int, int>();
		}

		public void Setup(Func<int, IUnit> unitGetter) => _instance._unitGetter = unitGetter;

		public static void AddUnit(int oldId, int newId)
		{
			if (_instance._oldUnitIdToNewUnitIdMap.ContainsKey(oldId))
			{
				Logger.LogError($"Unit with id {oldId} already exists");
				return;
			}

			_instance._oldUnitIdToNewUnitIdMap.Add(oldId, newId);
		}

		public static IUnit GetUnit(int oldId)
		{
			if (_instance._oldUnitIdToNewUnitIdMap.TryGetValue(oldId, out int newId))
			{
				Logger.Log($"Old id: {oldId}, new id: {newId}");
				return _instance._unitGetter(newId);
			}

			Logger.LogError($"Unit with id {oldId} not found");
			return null;
		}

		public void Reset()
		{
			_oldUnitIdToNewUnitIdMap.Clear();
			_unitGetter = null;
			_instance = null;
		}
	}
}