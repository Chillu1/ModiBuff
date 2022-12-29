using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ModifierIdManager
	{
		public static int CurrentId { get; private set; }

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

		public static int GetFreeId(string name)
		{
			CurrentId = _instance._nextId;
			int id = _instance._nextId++;
			if (!_instance._idMap.ContainsKey(name)) //TODO We keep making new recipes for every unit test file
				_instance._idMap.Add(name, id);
			return id;
		}

		public static int GetId(string name) => _instance._idMap[name];

		public static void Reset() => _instance._nextId = 0;
	}
}