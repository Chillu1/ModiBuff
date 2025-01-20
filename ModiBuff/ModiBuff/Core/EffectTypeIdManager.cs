using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class EffectTypeIdManager
	{
		private readonly Dictionary<Type, int> _effectTypeIds;
		private readonly Dictionary<Type, int> _metaEffectTypeIds;
		private readonly Dictionary<Type, int> _postEffectTypeIds;
		private readonly Dictionary<Type, int> _conditionTypeIds;

		public static EffectTypeIdManager Instance { get; private set; } //TODO TEMP

		private int _currentId, _metaCurrentId, _postCurrentId, _conditionCurrentId;

		public EffectTypeIdManager()
		{
			if (Instance != null)
				return;

			Instance = this;

			_effectTypeIds = new Dictionary<Type, int>();
			_metaEffectTypeIds = new Dictionary<Type, int>();
			_postEffectTypeIds = new Dictionary<Type, int>();
			_conditionTypeIds = new Dictionary<Type, int>();
		}

		public void RegisterEffectTypes(params Type[] types)
		{
			foreach (var type in types)
				RegisterEffectType(type);
		}

		public void RegisterMetaEffectTypes(params Type[] types)
		{
			foreach (var type in types)
				RegisterMetaEffectType(type);
		}

		public void RegisterPostEffectTypes(params Type[] types)
		{
			foreach (var type in types)
				RegisterPostEffectType(type);
		}

		public void RegisterConditionTypes(params Type[] types)
		{
			foreach (var type in types)
				RegisterConditionType(type);
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

		public void RegisterMetaEffectType(Type type)
		{
			if (_metaEffectTypeIds.ContainsKey(type))
			{
				Logger.LogError($"[ModiBuff] Meta effect type {type} already registered");
				return;
			}

			_metaEffectTypeIds.Add(type, _metaCurrentId++);
		}

		public void RegisterPostEffectType(Type type)
		{
			if (_postEffectTypeIds.ContainsKey(type))
			{
				Logger.LogError($"[ModiBuff] Post effect type {type} already registered");
				return;
			}

			_postEffectTypeIds.Add(type, _postCurrentId++);
		}

		public void RegisterConditionType(Type type)
		{
			if (_conditionTypeIds.ContainsKey(type))
			{
				Logger.LogError($"[ModiBuff] Condition type {type} already registered");
				return;
			}

			_conditionTypeIds.Add(type, _conditionCurrentId++);
		}

		public void RegisterAllEffectTypesInAssemblies()
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			foreach (var type in assembly.GetTypes())
			{
				if (!type.IsClass || type.IsAbstract)
					continue;

				if (typeof(IEffect).IsAssignableFrom(type))
					RegisterEffectType(type);
				if (typeof(IMetaEffect).IsAssignableFrom(type))
					RegisterMetaEffectType(type);
				if (typeof(IPostEffect).IsAssignableFrom(type))
					RegisterPostEffectType(type);
				if (typeof(ICondition).IsAssignableFrom(type))
					RegisterConditionType(type);
			}
		}

		public int GetId(Type type)
		{
			if (_effectTypeIds.TryGetValue(type, out int id))
				return id;

			Logger.LogError($"[ModiBuff] Effect type {type} not registered");
			return -1;
		}

		public int GetMetaId(Type type)
		{
			if (_metaEffectTypeIds.TryGetValue(type, out int id))
				return id;

			Logger.LogError($"[ModiBuff] Meta effect type {type} not registered");
			return -1;
		}

		public int GetPostId(Type type)
		{
			if (_postEffectTypeIds.TryGetValue(type, out int id))
				return id;

			Logger.LogError($"[ModiBuff] Post effect type {type} not registered");
			return -1;
		}

		public int GetConditionId(Type type)
		{
			if (_conditionTypeIds.TryGetValue(type, out int id))
				return id;

			Logger.LogError($"[ModiBuff] Condition type {type} not registered");
			return -1;
		}

		public Type GetEffectType(int id)
		{
			foreach (var pair in _effectTypeIds)
			{
				if (pair.Value == id)
					return pair.Key;
			}

			Logger.LogError($"[ModiBuff] Effect type with id {id} not registered");
			return null;
		}

		public Type GetMetaEffectType(int id)
		{
			foreach (var pair in _metaEffectTypeIds)
			{
				if (pair.Value == id)
					return pair.Key;
			}

			Logger.LogError($"[ModiBuff] Meta effect type with id {id} not registered");
			return null;
		}

		public Type GetPostEffectType(int id)
		{
			foreach (var pair in _postEffectTypeIds)
			{
				if (pair.Value == id)
					return pair.Key;
			}

			Logger.LogError($"[ModiBuff] Post effect type with id {id} not registered");
			return null;
		}

		public Type GetConditionType(int id)
		{
			foreach (var pair in _conditionTypeIds)
			{
				if (pair.Value == id)
					return pair.Key;
			}

			Logger.LogError($"[ModiBuff] Condition type with id {id} not registered");
			return null;
		}

		public bool MatchesId(Type type, int id)
		{
			if (_effectTypeIds.TryGetValue(type, out int typeId))
				return typeId == id;

			Logger.LogError($"[ModiBuff] Effect type {type} not registered");
			return false;
		}

		public bool MatchesMetaId(Type type, int id)
		{
			if (_metaEffectTypeIds.TryGetValue(type, out int typeId))
				return typeId == id;

			Logger.LogError($"[ModiBuff] Meta effect type {type} not registered");
			return false;
		}

		public bool MatchesPostId(Type type, int id)
		{
			if (_postEffectTypeIds.TryGetValue(type, out int typeId))
				return typeId == id;

			Logger.LogError($"[ModiBuff] Post effect type {type} not registered");
			return false;
		}

		public bool MatchesConditionId(Type type, int id)
		{
			if (_conditionTypeIds.TryGetValue(type, out int typeId))
				return typeId == id;

			Logger.LogError($"[ModiBuff] Condition type {type} not registered");
			return false;
		}

		public void Reset()
		{
			Instance = null;
			_effectTypeIds.Clear();
			_metaEffectTypeIds.Clear();
			_postEffectTypeIds.Clear();
			_conditionTypeIds.Clear();
			_currentId = _metaCurrentId = _postCurrentId = _conditionCurrentId = 0;
		}
	}
}