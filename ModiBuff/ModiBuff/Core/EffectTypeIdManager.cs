using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class EffectTypeIdManager<T>
	{
		private readonly string _name;
		private readonly Dictionary<Type, int> _typeIds;

		public static EffectTypeIdManager<T> Instance { get; private set; } //TODO TEMP

		private int _currentId;

		public EffectTypeIdManager(string name)
		{
			if (Instance != null)
				return;

			Instance = this;

			_name = name;
			_typeIds = new Dictionary<Type, int>();
		}

		public void RegisterType(Type type)
		{
			if (_typeIds.ContainsKey(type))
			{
				Logger.LogError($"[ModiBuff] {_name} type {type} already registered");
				return;
			}

			_typeIds.Add(type, _currentId++);
		}

		public void RegisterAllEffectTypesInAssemblies(Type[] types)
		{
			foreach (var type in types)
			{
				if (!type.IsClass || type.IsAbstract)
					continue;

				if (typeof(T).IsAssignableFrom(type))
					RegisterType(type);
			}
		}

		public int GetId(Type type)
		{
			if (_typeIds.TryGetValue(type, out int id))
				return id;

			Logger.LogError($"[ModiBuff] {_name} type {type} not registered");
			return -1;
		}

		public Type GetType(int id)
		{
			foreach (var pair in _typeIds)
			{
				if (pair.Value == id)
					return pair.Key;
			}

			Logger.LogError($"[ModiBuff] {_name} type with id {id} not registered");
			return null;
		}

		public bool MatchesId(Type type, int id)
		{
			if (_typeIds.TryGetValue(type, out int typeId))
				return typeId == id;

			Logger.LogError($"[ModiBuff] {_name} type {type} not registered");
			return false;
		}

		public void Reset()
		{
			Instance = null;
			_typeIds.Clear();
			_currentId = 0;
		}
	}

	public sealed class EffectTypeIdManager
	{
		private readonly EffectTypeIdManager<IEffect> _effects;
		private readonly EffectTypeIdManager<IMetaEffect> _metaEffects;
		private readonly EffectTypeIdManager<IPostEffect> _postEffects;
		private readonly EffectTypeIdManager<ICondition> _conditions;

		public EffectTypeIdManager()
		{
			_effects = new EffectTypeIdManager<IEffect>("Effect");
			_metaEffects = new EffectTypeIdManager<IMetaEffect>("Meta effect");
			_postEffects = new EffectTypeIdManager<IPostEffect>("Post effect");
			_conditions = new EffectTypeIdManager<ICondition>("Condition");
		}

		public void RegisterAllEffectTypesInAssemblies()
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				var types = assembly.GetTypes();
				_effects.RegisterAllEffectTypesInAssemblies(types);
				_metaEffects.RegisterAllEffectTypesInAssemblies(types);
				_postEffects.RegisterAllEffectTypesInAssemblies(types);
				_conditions.RegisterAllEffectTypesInAssemblies(types);
			}
		}

		public int GetId(Type type) => _effects.GetId(type);

		public Type GetEffectType(int id) => _effects.GetType(id);

		public Type GetMetaEffectType(int id) => _metaEffects.GetType(id);

		public Type GetPostEffectType(int id) => _postEffects.GetType(id);

		public Type GetConditionType(int id) => _conditions.GetType(id);

		public bool MatchesId(Type type, int id) => _effects.MatchesId(type, id);

		public void Reset()
		{
			_effects.Reset();
			_metaEffects.Reset();
			_postEffects.Reset();
			_conditions.Reset();
		}
	}
}