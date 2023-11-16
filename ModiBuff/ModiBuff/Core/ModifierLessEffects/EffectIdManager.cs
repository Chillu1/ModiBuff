using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class EffectIdManager
	{
		private static EffectIdManager _instance;
		private int _nextId;

		private readonly Dictionary<string, int> _idMap;
		private readonly Dictionary<int, int> _oldIdToNewIdMap;

		public EffectIdManager()
		{
			if (_instance != null)
				return;

			_instance = this;
			_nextId = 0;
			_idMap = new Dictionary<string, int>();
			_oldIdToNewIdMap = new Dictionary<int, int>();
		}

		public int GetFreeId(string name)
		{
			int id = _nextId;
			if (!_idMap.ContainsKey(name))
				_idMap.Add(name, id);
			_nextId++;
			return id;
		}

		/// <summary>
		///		Lazy implementation for ease of use.
		/// </summary>
		internal static int GetIdOld(string name) => _instance.GetId(name);

		internal static bool HasIdOld(string name) => _instance.HasId(name);
		internal bool HasId(string name) => _idMap.ContainsKey(name);

		public int GetId(string name)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (!_idMap.ContainsKey(name))
			{
				if (!ModifierIdManager.HasIdByName(name))
					Logger.LogError("[ModiBuff] No effect with name " + name + " found.");
				else
					Logger.LogError("[ModiBuff] No effect with name " + name + " found. " +
					                "But there is a modifier with that name. Did you mean to use ModifierIdManager?");
				return -1;
			}
#endif

			return _idMap[name];
		}

		public static int GetNewId(int oldId)
		{
			if (_instance._oldIdToNewIdMap.TryGetValue(oldId, out int newId))
				return newId;

			Logger.LogError($"[ModiBuff] Modifier with id {oldId} not found");
			return -1;
		}

		public void Clear()
		{
			_nextId = 0;
			_idMap.Clear();
			_oldIdToNewIdMap.Clear();
		}

		public void Reset() => _instance = null;

		public SaveData SaveState() => new SaveData(_idMap);

		public void LoadState(SaveData saveData)
		{
			foreach (var pair in saveData.IdMap)
			{
				if (_idMap.TryGetValue(pair.Key, out int newId))
					_oldIdToNewIdMap.Add(pair.Value, newId);
				else
					Logger.LogError($"[ModiBuff] Effect in save file with name {pair.Key} not found.");
			}
		}

		public readonly struct SaveData
		{
			public readonly IReadOnlyDictionary<string, int> IdMap;

#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET462_OR_GREATER || NETCOREAPP2_1_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(IReadOnlyDictionary<string, int> idMap) => IdMap = idMap;
		}
	}
}