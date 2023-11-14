using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class EffectTypeIdManager
	{
		public static EffectTypeIdManager Instance { get; private set; }

		private readonly Dictionary<Type, int> _effectTypeIds;

		private int _currentId;

		public EffectTypeIdManager()
		{
			if (Instance != null)
				return;

			Instance = this;

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
			Instance = null;
		}
	}
}