using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class EffectTypeIdManager
	{
		private static EffectTypeIdManager _instance;

		private readonly Dictionary<Type, int> _effectTypeIds;

		private int _currentId;

		public EffectTypeIdManager()
		{
			if (_instance != null)
				return;

			_instance = this;

			_effectTypeIds = new Dictionary<Type, int>();
		}

		public void RegisterEffectTypes(params Type[] types)
		{
			foreach (var type in types)
				RegisterEffectType(type);
		}

		public void RegisterEffectType(Type type)
		{
			if (_effectTypeIds.ContainsKey(type))
			{
				Logger.LogError($"[ModiBuff] Effect type {type} already registered");
				return;
			}

			_effectTypeIds.Add(type, _currentId++);
		}

		public int GetId(Type type)
		{
			if (_effectTypeIds.TryGetValue(type, out int id))
				return id;

			Logger.LogError($"[ModiBuff] Effect type {type} not registered");
			return -1;
		}

		public Type GetEffectType(int id)
		{
			foreach (var pair in _effectTypeIds)
			{
				if (pair.Value == id)
					return pair.Key;
			}

			Logger.LogWarning($"[ModiBuff] Effect type with id {id} not registered");
			return null;
		}

		public bool MatchesId(Type type, int id)
		{
			if (_effectTypeIds.TryGetValue(type, out int typeId))
				return typeId == id;

			Logger.LogError($"[ModiBuff] Effect type {type} not registered");
			return false;
		}

		public void Reset()
		{
			_effectTypeIds.Clear();
			_instance = null;
		}
	}
}