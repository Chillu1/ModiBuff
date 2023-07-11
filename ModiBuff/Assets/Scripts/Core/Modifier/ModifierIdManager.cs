using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ModifierIdManager
	{
		public static int NextId => _instance._nextId;

		private static ModifierIdManager _instance;
		private int _nextId;

		private Dictionary<string, int> _idMap;

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
			_idMap.TryAdd(name, id);
			_nextId++;
			return id;
		}

		/// <summary>
		///		Lazy implementation for ease of use.
		/// </summary>
		internal static int GetIdOld(string name) => _instance._idMap[name];

		public int GetId(string name) => _idMap[name];

		public void Reset() => _instance = null;
	}
}