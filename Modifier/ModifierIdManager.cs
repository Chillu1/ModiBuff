using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ModifierIdManager
	{
		private static ModifierIdManager _instance;
		private int _nextId;

		private readonly Dictionary<string, int> _idMap;
		private readonly Dictionary<int, int> _oldIdToNewIdMap;

		public ModifierIdManager()
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
		internal static int GetIdByName(string name) => _instance.GetId(name);

		internal static bool HasIdByName(string name) => _instance._idMap.ContainsKey(name);

		public int GetId(string name)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (!_idMap.ContainsKey(name))
			{
				if (!EffectIdManager.HasIdOld(name))
					Logger.LogError("[ModiBuff] No modifier with name " + name + " found.");
				else
					Logger.LogError("[ModiBuff] No modifier with name " + name + " found. " +
					                "But there is an effect with that name. Did you mean to use EffectIdManager?");
				return -1;
			}
#endif

			return _idMap[name];
		}

		public static int GetNewId(int oldId)
		{
			if (_instance._oldIdToNewIdMap.TryGetValue(oldId, out int newId))
				return newId;

			Logger.LogError($"Modifier with id {oldId} not found");
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
					Logger.LogError($"[ModiBuff] Modifier in save file with name {pair.Key} not found.");
			}
		}

		public readonly struct SaveData
		{
			public readonly IReadOnlyDictionary<string, int> IdMap;

#if MODIBUFF_SYSTEM_TEXT_JSON
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(IReadOnlyDictionary<string, int> idMap) => IdMap = idMap;
		}
	}
}