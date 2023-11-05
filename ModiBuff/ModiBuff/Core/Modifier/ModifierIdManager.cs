using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ModifierIdManager
	{
		private static ModifierIdManager _instance;
		private int _nextId;

		private readonly Dictionary<string, int> _idMap;

		public ModifierIdManager()
		{
			if (_instance != null)
				return;

			_instance = this;
			_nextId = 0;
			_idMap = new Dictionary<string, int>();
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

		internal static bool HasIdOld(string name) => _instance._idMap.ContainsKey(name);

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

		public void Clear()
		{
			_nextId = 0;
			_idMap.Clear();
		}

		public void Reset() => _instance = null;
	}
}