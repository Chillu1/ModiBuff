using System;
using System.Collections.Generic;
using System.Linq;

namespace ModiBuff.Core
{
	public abstract class ModifierRecipes
	{
		public static int RecipesCount { get; private set; }

		private static ModifierRecipes _instance; //TODO TEMP

		private readonly ModifierIdManager _idManager;
		private readonly IDictionary<string, IModifierRecipe> _recipes;
		private readonly IDictionary<string, IModifierGenerator> _modifierGenerators;
		private readonly List<RegisterData> _registeredNames;
		private readonly bool[] _instanceStackableIds;

		private EventEffectFactory _eventEffectFunc;

		public ModifierRecipes(ModifierIdManager idManager)
		{
			_instance = this;

			_idManager = idManager;
			_recipes = new Dictionary<string, IModifierRecipe>(64);
			_modifierGenerators = new Dictionary<string, IModifierGenerator>(64);
			_registeredNames = new List<RegisterData>(16);

			SetupRecipes();
			_instanceStackableIds = new bool[_recipes.Count];
			foreach (var recipe in _recipes.Values)
			{
				if (recipe is ModifierRecipe modifierRecipe && modifierRecipe.IsInstanceStackable)
					_instanceStackableIds[modifierRecipe.Id] = true;
				_modifierGenerators.Add(recipe.Name, recipe.CreateModifierGenerator());
			}

			RecipesCount = _recipes.Count;
#if DEBUG && !MODIBUFF_PROFILE
			Logger.Log($"[ModiBuff] Loaded {RecipesCount} recipes.");
#endif
		}

		protected abstract void SetupRecipes();

		/// <summary>
		///		Call this, and feed an event effect func factory to use the event recipes.
		/// </summary>
		protected void SetupEventEffect(EventEffectFactory eventEffectFunc) => _eventEffectFunc = eventEffectFunc;

		public static bool IsInstanceStackable(int id) => _instance._instanceStackableIds[id];

		public IModifierGenerator GetGenerator(string id) => _modifierGenerators[id];

		internal IModifierGenerator[] GetGenerators() => _modifierGenerators.Values.ToArray();

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

		protected ModifierEventRecipe AddEvent<T>(string name, T effectOnEvent) => AddEvent(name, (int)(object)effectOnEvent);

		protected ModifierEventRecipe AddEvent(string name, int effectOnEvent)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (_eventEffectFunc == null)
				Logger.LogError("Event effect func is not set up. But you are trying to create an event recipe.");
#endif

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

			var recipe = new ModifierEventRecipe(id, name, effectOnEvent, _eventEffectFunc);
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

		private readonly struct RegisterData
		{
			public readonly string Name;
			public readonly int Id;

			public RegisterData(string name, int id)
			{
				Name = name;
				Id = id;
			}
		}
	}
}