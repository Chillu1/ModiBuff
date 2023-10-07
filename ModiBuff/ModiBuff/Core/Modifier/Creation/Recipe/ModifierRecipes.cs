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

		private ModifierAddData[] _modifierAddData;
		private ModifierInfo[] _modifierInfos;
		private int[] _tags;

		private EventEffectFactory _eventEffectFunc;

		public ModifierRecipes(ModifierIdManager idManager, EventEffectFactory eventEffectFunc = null)
		{
			_instance = this;

			_idManager = idManager;
			_recipes = new Dictionary<string, IModifierRecipe>(64);
			_manualGenerators = new Dictionary<string, ManualModifierGenerator>(64);
			_modifierGenerators = new Dictionary<string, IModifierGenerator>(64);
			_registeredNames = new List<RegisterData>(16);

			_eventEffectFunc = eventEffectFunc;
		}

		protected virtual void SetupRecipes()
		{
		}

		/// <summary>
		///		Finishes the setup of the recipes, and creates the generators.
		/// </summary>
		public void CreateGenerators()
		{
			SetupRecipes();

			_modifierAddData = new ModifierAddData[_recipes.Count + _manualGenerators.Count];
			_modifierInfos = new ModifierInfo[_recipes.Count + _manualGenerators.Count];
			_tags = new int[_recipes.Count + _manualGenerators.Count];
			foreach (var generator in _manualGenerators.Values)
			{
				_modifierAddData[generator.Id] = generator.GetAddData();
				_modifierGenerators.Add(generator.Name, generator);
				//TODO Info from manual generators
				_modifierInfos[generator.Id] = new ModifierInfo(generator.Id, generator.Name, generator.Name, "");
				_tags[generator.Id] = generator.Tag;
			}

			foreach (var recipe in _recipes.Values)
			{
				_modifierAddData[recipe.Id] = recipe.CreateAddData();
				_modifierGenerators.Add(recipe.Name, recipe.CreateModifierGenerator());
				_modifierInfos[recipe.Id] = recipe.CreateModifierInfo();
			}

			//_modifierInfos = new ModifierInfo[_modifierGenerators.Count];
			//foreach (var generator in _modifierGenerators.Values)
			//{
			//	//generator.CreateModifierInfo();
			//	_modifierInfos[generator.Id] = new ModifierInfo(generator.Id, generator.Name);
			//}

			GeneratorCount = _modifierGenerators.Count;
#if DEBUG && !MODIBUFF_PROFILE
			Logger.Log($"[ModiBuff] Loaded {GeneratorCount} modifier generators.");
#endif
		}

		public ModifierInfo GetModifierInfo(int id)
		{
			if (id < 0 || id >= _modifierInfos.Length)
			{
				Logger.LogError($"Modifier with id {id} does not exist.");
				return null;
			}

			return _modifierInfos[id];
		}

		/// <summary>
		///		Call this, and feed an event effect func factory to use the event recipes.
		/// </summary>
		public void SetupEventEffect<TEvent>(EventEffectFactory eventEffectFunc)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (eventEffectFunc(new IEffect[0], default(TEvent)) is IRevertEffect revertEffect &&
			    revertEffect.IsRevertible)
				Logger.LogError("Event effect func does not return an effect that implements " +
				                "IRevertEffect, or is not revertible.");
#endif

			_eventEffectFunc = eventEffectFunc;
		}

		public static ref readonly ModifierAddData GetAddData(int id) => ref _instance._modifierAddData[id];
		public static int GetTag(int id) => _instance._tags[id];

		public IModifierGenerator GetGenerator(string name) => _modifierGenerators[name];

		internal IModifierGenerator[] GetGenerators() => _modifierGenerators.Values.ToArray();

		public ModifierRecipe Add(string name, string displayName = "", string description = "")
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

			if (string.IsNullOrEmpty(displayName))
				displayName = name;
			var recipe = new ModifierRecipe(id, name, displayName, description, _idManager);
			_recipes.Add(name, recipe);
			return recipe;
		}

		public void Add<TTag>(in ManualGeneratorData<TTag> data) =>
			Add(data.Name, in data.CreateFunc, in data.AddData, data.Tag);

		public void Add<TTag>(string name, in ModifierGeneratorFunc createFunc,
			in ModifierAddData addData, TTag tag = default)
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

			var modifierGenerator = new ManualModifierGenerator(id, name, in createFunc, in addData, (int)(object)tag);
			_manualGenerators.Add(name, modifierGenerator);
		}

		public ModifierEventRecipe AddEvent<T>(string name, T effectOnEvent) => AddEvent(name, (object)effectOnEvent);

		public ModifierEventRecipe AddEvent(string name, object effectOnEvent)
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
		public void Register(params string[] names)
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