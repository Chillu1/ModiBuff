using System.Collections.Generic;

namespace ModiBuff.Core
{
	public static class UnitHelper
	{
		private delegate bool TryParse<T>(out T result);

		public static IUnit? GetUnit(object oldId)
		{
			if (TryGetUnit((out ulong id) => ulong.TryParse(oldId.ToString(), out id), out IUnit? unit))
				return unit;
			if (TryGetUnit((out long id) => long.TryParse(oldId.ToString(), out id), out unit))
				return unit;
			if (TryGetUnit((out uint id) => uint.TryParse(oldId.ToString(), out id), out unit))
				return unit;
			if (TryGetUnit((out int id) => int.TryParse(oldId.ToString(), out id), out unit))
				return unit;
			if (TryGetUnit((out ushort id) => ushort.TryParse(oldId.ToString(), out id), out unit))
				return unit;
			if (TryGetUnit((out short id) => short.TryParse(oldId.ToString(), out id), out unit))
				return unit;
			if (TryGetUnit((out sbyte id) => sbyte.TryParse(oldId.ToString(), out id), out unit))
				return unit;
			if (TryGetUnit((out byte id) => byte.TryParse(oldId.ToString(), out id), out unit))
				return unit;

			Logger.LogError($"[ModiBuff] Unit with id {oldId} not found in any UnitHelper instance");
			return null;

			bool TryGetUnit<T>(TryParse<T> parse, out IUnit? unit)
			{
				if (UnitHelper<T>.IsInstanceCreated)
				{
					if (oldId is T id)
					{
						unit = UnitHelper<T>.GetUnit(id);
						return true;
					}

					if (oldId is string && parse(out T? parseId))
					{
						unit = UnitHelper<T>.GetUnit(parseId);
						return true;
					}
				}

				unit = null;
				return false;
			}
		}
	}

	public sealed class UnitHelper<TId>
	{
		public static bool IsInstanceCreated => _instance != null;

		private static UnitHelper<TId>? _instance;

		private readonly Dictionary<TId, TId> _oldUnitIdToNewUnitIdMap;
		private readonly Dictionary<TId, IUnit> _idToUnitMap;

		public UnitHelper()
		{
			_instance = this;
			_idToUnitMap = new Dictionary<TId, IUnit>();
			_oldUnitIdToNewUnitIdMap = new Dictionary<TId, TId>();
		}

		public void AddUnit(IUnit unit, TId id)
		{
			if (_idToUnitMap.ContainsKey(id))
			{
				Logger.LogError($"[ModiBuff] Unit with id {id} already exists");
				return;
			}

			_idToUnitMap.Add(id, unit);
		}

		public static void LoadUnit(IUnit unit, TId oldId, TId newId)
		{
			if (_instance!._oldUnitIdToNewUnitIdMap.ContainsKey(oldId))
			{
				Logger.LogError($"[ModiBuff] Unit with id {oldId} already exists");
				return;
			}

			_instance._oldUnitIdToNewUnitIdMap.Add(oldId, newId);
			_instance._idToUnitMap.Add(newId, unit);
		}

		public static IUnit? GetUnit(TId oldId)
		{
			if (_instance!._oldUnitIdToNewUnitIdMap.TryGetValue(oldId, out TId newId))
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