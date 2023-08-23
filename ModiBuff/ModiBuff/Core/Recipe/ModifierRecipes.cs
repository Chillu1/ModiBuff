using System.Collections.Generic;
using System.Linq;

namespace ModiBuff.Core
{
	public abstract class ModifierRecipes
	{
		public static int RecipesCount { get; private set; }

		private readonly ModifierIdManager _idManager;
		private readonly IDictionary<string, IModifierRecipe> _recipes;
		private readonly List<RegisterData> _registeredNames;

		public ModifierRecipes(ModifierIdManager idManager)
		{
			_idManager = idManager;
			_recipes = new Dictionary<string, IModifierRecipe>(64);
			_registeredNames = new List<RegisterData>(16);

			SetupRecipes();
			foreach (var modifier in _recipes.Values)
				modifier.Finish();

			RecipesCount = _recipes.Count;
#if DEBUG && !MODIBUFF_PROFILE
			Logger.Log($"[ModiBuff] Loaded {RecipesCount} recipes.");
#endif
		}

		protected abstract void SetupRecipes();

		public IModifierRecipe GetRecipe(string id) => _recipes[id];
		internal IModifierRecipe GetRecipe(int id) => _recipes.Values.ElementAt(id);

		internal IModifierRecipe[] GetRecipes() => _recipes.Values.ToArray();

		protected ModifierRecipe Add(string name)
		{
			if (_recipes.TryGetValue(name, out var localRecipe))
			{
#if DEBUG && !MODIBUFF_PROFILE
				Logger.LogError($"Modifier with id {name} already exists");
#endif
				return (ModifierRecipe)localRecipe;
			}

			int id = -1;

			foreach (var registerData in _registeredNames)
			{
				if (registerData.Name == name)
				{
					id = registerData.Id;
					break;
				}
			}

			if (id == -1)
				id = _idManager.GetFreeId(name);

			var recipe = new ModifierRecipe(id, name, _idManager);
			_recipes.Add(name, recipe);
			return recipe;
		}

		protected ModifierEventRecipe AddEvent(string name, EffectOnEvent effectOnEvent)
		{
			if (_recipes.TryGetValue(name, out var localRecipe))
			{
#if DEBUG && !MODIBUFF_PROFILE
				Logger.LogError($"Modifier with id {name} already exists");
#endif
				return (ModifierEventRecipe)localRecipe;
			}

			int id = -1;

			foreach (var registerData in _registeredNames)
			{
				if (registerData.Name == name)
				{
					id = registerData.Id;
					break;
				}
			}

			if (id == -1)
				id = _idManager.GetFreeId(name);

			var recipe = new ModifierEventRecipe(id, name, effectOnEvent);
			_recipes.Add(name, recipe);
			return recipe;
		}

		/// <summary>
		///		Special case when we want to use applier/conditional modifiers and want to set them up in order.
		///		This can also be used to have the first original modifier take the first available id.
		/// </summary>
		protected void Register(params string[] names)
		{
			foreach (string name in names)
			{
				if (_registeredNames.Any(tuple => tuple.Name == name))
				{
#if DEBUG && !MODIBUFF_PROFILE
					Logger.LogError($"Modifier with id {name} already exists");
#endif
					continue;
				}

				_registeredNames.Add(new RegisterData(name, _idManager.GetFreeId(name)));
			}
		}

		private struct RegisterData
		{
			public string Name;
			public int Id;

			public RegisterData(string name, int id)
			{
				Name = name;
				Id = id;
			}
		}
	}
}