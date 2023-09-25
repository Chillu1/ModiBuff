using System.Collections.Generic;

namespace ModiBuff.Core.Units
{
	public sealed class UnitIdManager
	{
		private static UnitIdManager _instance;
		private int _nextId;

		private Dictionary<string, int> _idMap;

		public UnitIdManager()
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
			if(!_idMap.ContainsKey(name))
				_idMap.Add(name, id);
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