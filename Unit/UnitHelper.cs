using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class UnitHelper
	{
		private static UnitHelper _instance;

		private readonly Dictionary<int, int> _oldUnitIdToNewUnitIdMap;
		private readonly Dictionary<int, IUnit> _idToUnitMap;

		public UnitHelper()
		{
			_instance = this;
			_idToUnitMap = new Dictionary<int, IUnit>();
			_oldUnitIdToNewUnitIdMap = new Dictionary<int, int>();
		}

		public void AddUnit(IUnit unit, int id)
		{
			if (_idToUnitMap.ContainsKey(id))
			{
				Logger.LogError($"[ModiBuff] Unit with id {id} already exists");
				return;
			}

			_idToUnitMap.Add(id, unit);
		}

		public static void LoadUnit(IUnit unit, int oldId, int newId)
		{
			if (_instance._oldUnitIdToNewUnitIdMap.ContainsKey(oldId))
			{
				Logger.LogError($"[ModiBuff] Unit with id {oldId} already exists");
				return;
			}

			_instance._oldUnitIdToNewUnitIdMap.Add(oldId, newId);
			_instance._idToUnitMap.Add(newId, unit);
		}

		public static IUnit GetUnit(int oldId)
		{
			if (_instance._oldUnitIdToNewUnitIdMap.TryGetValue(oldId, out int newId))
				return _instance._idToUnitMap[newId];

			Logger.LogError($"[ModiBuff] Unit with id {oldId} not found");
			return null;
		}

		public void Reset()
		{
			_oldUnitIdToNewUnitIdMap.Clear();
			_idToUnitMap.Clear();
			_instance = null;
		}
	}
}