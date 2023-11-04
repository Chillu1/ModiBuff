using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class EffectIdManager
	{
		private static EffectIdManager _instance;
		private int _nextId;

		private readonly Dictionary<string, int> _idMap;

		public EffectIdManager()
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
		internal static int GetIdOld(string name) => _instance._idMap[name];

		public bool IsIdTaken(string name) => _idMap.ContainsKey(name);

		public int GetId(string name) => _idMap[name];

		public void Clear()
		{
			_nextId = 0;
			_idMap.Clear();
		}

		public void Reset() => _instance = null;
	}
}