using System;
using System.Collections.Generic;
using System.Linq;

namespace ModiBuff.Core
{
	public delegate ModifierRecipe RecipeAddFunc(Func<string, ModifierRecipe> addFunc);

	public delegate ModifierEventRecipe EventRecipeAddFunc(Func<string, object, ModifierEventRecipe> addFunc);

	public class ModifierRecipes
	{
		public static int GeneratorCount { get; private set; }

		private static ModifierRecipes _instance; //TODO TEMP

		private readonly ModifierIdManager _idManager;
		private readonly IDictionary<string, IModifierRecipe> _recipes;
		private readonly IDictionary<string, ManualModifierGenerator> _manualGenerators;
		private readonly IDictionary<string, IModifierGenerator> _modifierGenerators;
		private readonly List<RegisterData> _registeredNames;
		private readonly ModifierAddData[] _modifierAddData;

		private EventEffectFactory _eventEffectFunc;

		//TODO Refactor Unit tests recipe adding, and combines constructors
		internal ModifierRecipes(IReadOnlyList<RecipeAddFunc> recipes, IReadOnlyList<EventRecipeAddFunc> eventRecipes,
			ModifierIdManager idManager, EventEffectFactory eventEffectFunc, ManualGeneratorData[] manualGeneratorData = null)
		{
			_instance = this;

			_idManager = idManager;
			_eventEffectFunc = eventEffectFunc;
			_recipes = new Dictionary<string, IModifierRecipe>(64);
			_manualGenerators = new Dictionary<string, ManualModifierGenerator>(64);
			_modifierGenerators = new Dictionary<string, IModifierGenerator>(64);
			_registeredNames = new List<RegisterData>(16);

			for (int i = 0; i < recipes.Count; i++)
			{
				recipes[i](delegate(string name)
				{
					Register(name);
					//TODO Terrible hack for terrible design, needs refactor ASAP
					return new ModifierRecipe(0, name, _idManager);
				});
			}

			for (int i = 0; i < eventRecipes.Count; i++)
			{
				eventRecipes[i](delegate(string name, object @event)
				{
					Register(name);
					//TODO Terrible hack for terrible design, needs refactor ASAP
					return new ModifierEventRecipe(0, name, _idManager, null);
				});
			}

			for (int i = 0; i < recipes.Count; i++)
				recipes[i](Add);
			for (int i = 0; i < eventRecipes.Count; i++)
				eventRecipes[i](AddEvent);

			for (int i = 0; i < manualGeneratorData?.Length; i++)
				Add(manualGeneratorData[i]);

			SetupRecipes();
			_modifierAddData = new ModifierAddData[_recipes.Count + _manualGenerators.Count];
			foreach (var generator in _manualGenerators.Values)
			{
				_modifierAddData[generator.Id] = generator.GetAddData();
				_modifierGenerators.Add(generator.Name, generator);
			}

			foreach (var recipe in _recipes.Values)
			{
				_modifierAddData[recipe.Id] = recipe.CreateAddData();
				_modifierGenerators.Add(recipe.Name, recipe.CreateModifierGenerator());
			}

			GeneratorCount = _modifierGenerators.Count;
#if DEBUG && !MODIBUFF_PROFILE
			Logger.Log($"[ModiBuff] Loaded {GeneratorCount} modifier generators.");
#endif
		}

		protected ModifierRecipes(ModifierIdManager idManager)
		{
			_instance = this;

			_idManager = idManager;
			_recipes = new Dictionary<string, IModifierRecipe>(64);
			_manualGenerators = new Dictionary<string, ManualModifierGenerator>(64);
			_modifierGenerators = new Dictionary<string, IModifierGenerator>(64);
			_registeredNames = new List<RegisterData>(16);

			SetupRecipes();
			_modifierAddData = new ModifierAddData[_recipes.Count + _manualGenerators.Count];
			foreach (var generator in _manualGenerators.Values)
			{
				_modifierAddData[generator.Id] = generator.GetAddData();
				_modifierGenerators.Add(generator.Name, generator);
			}

			foreach (var recipe in _recipes.Values)
			{
				_modifierAddData[recipe.Id] = recipe.CreateAddData();
				_modifierGenerators.Add(recipe.Name, recipe.CreateModifierGenerator());
			}

			GeneratorCount = _modifierGenerators.Count;
#if DEBUG && !MODIBUFF_PROFILE
			Logger.Log($"[ModiBuff] Loaded {GeneratorCount} modifier generators.");
#endif
		}

		protected virtual void SetupRecipes()
		{
		}

		/// <summary>
		///		Call this, and feed an event effect func factory to use the event recipes.
		/// </summary>
		public void SetupEventEffect<TEvent>(EventEffectFactory eventEffectFunc)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (eventEffectFunc(new IEffect[0], default(TEvent)) is IRevertEffect revertEffect && revertEffect.IsRevertible)
				Logger.LogError("Event effect func does not return an effect that implements IRevertEffect, or is not revertible.");
#endif

			_eventEffectFunc = eventEffectFunc;
		}

		public static ref readonly ModifierAddData GetAddData(int id) => ref _instance._modifierAddData[id];

		public IModifierGenerator GetGenerator(string id) => _modifierGenerators[id];

		internal IModifierGenerator[] GetGenerators() => _modifierGenerators.Values.ToArray();

		protected ModifierRecipe Add(string name)
		{
			if (_recipes.TryGetValue(name, out var localRecipe))
			{
#if DEBUG && !MODIBUFF_PROFILE
				Logger.LogError($"Modifier recipe with name {name} already exists");
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

		protected void Add(in ManualGeneratorData data) => Add(data.Name, in data.CreateFunc, in data.AddData);

		protected void Add(string name, in ModifierGeneratorFunc createFunc, in ModifierAddData addData)
		{
			if (_recipes.ContainsKey(name))
			{
#if DEBUG && !MODIBUFF_PROFILE
				Logger.LogError($"Modifier recipe with name {name} already exists");
#endif
				return;
			}

			if (_manualGenerators.ContainsKey(name))
			{
#if DEBUG && !MODIBUFF_PROFILE
				Logger.LogError($"Modifier generator with name {name} already exists");
#endif
				return;
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

			var modifierGenerator = new ManualModifierGenerator(id, name, in createFunc, in addData);
			_manualGenerators.Add(name, modifierGenerator);
		}

		protected ModifierEventRecipe AddEvent<T>(string name, T effectOnEvent) => AddEvent(name, (object)effectOnEvent);

		protected ModifierEventRecipe AddEvent(string name, object effectOnEvent)
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

		public void Clear()
		{
			_recipes.Clear();
			Array.Clear(_modifierAddData, 0, _modifierAddData.Length);
			_modifierGenerators.Clear();
			_registeredNames.Clear();
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