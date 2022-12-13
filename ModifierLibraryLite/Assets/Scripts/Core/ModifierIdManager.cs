using System.Collections.Generic;

namespace ModifierLibraryLite.Core
{
	public sealed class ModifierIdManager
	{
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

		public void SetupRecipeIds(ModifierRecipe[] modifierRecipes)
		{
			if (_idMap == null || _idMap.Count > 0)
				return;

			foreach (var recipe in modifierRecipes)
				_idMap.Add(recipe.Id, recipe.IdInt);
		}

		public static int GetFreeId() => _instance._nextId++;

		public static int GetId(string id) => _instance._idMap[id];

		public static void Reset() => _instance._nextId = 0;
	}
}