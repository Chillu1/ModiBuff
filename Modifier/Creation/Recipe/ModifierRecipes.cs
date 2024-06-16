using System;
using System.Collections.Generic;
using System.Linq;

namespace ModiBuff.Core
{
	public delegate ModifierRecipe RecipeAddFunc(Func<string, ModifierRecipe> addFunc);

	public interface IModifierRecipes
	{
		IModifierGenerator[] GetGenerators();
		IModifierGenerator GetGenerator(string name);
		ModifierInfo GetModifierInfo(int id);
	}

	public class ModifierRecipes : IModifierRecipes
	{
		public static int GeneratorCount { get; private set; }

		private static ModifierRecipes _instance; //TODO TEMP

		private readonly ModifierIdManager _idManager;
		private readonly IDictionary<string, IModifierRecipe> _recipes;
		private readonly IDictionary<string, ManualModifierGenerator> _manualGenerators;
		private readonly IDictionary<string, IModifierGenerator> _modifierGenerators;
		private readonly List<RegisterData> _registeredNames;

		private ModifierInfo[] _modifierInfos;
		private TagType[] _tags;

		public ModifierRecipes(ModifierIdManager idManager)
		{
			_instance = this;

			_idManager = idManager;
			_recipes = new Dictionary<string, IModifierRecipe>(64);
			_manualGenerators = new Dictionary<string, ManualModifierGenerator>(64);
			_modifierGenerators = new Dictionary<string, IModifierGenerator>(64);
			_registeredNames = new List<RegisterData>(16);
		}

		//TODO TEMP
		internal static void SetInstance(ModifierRecipes instance) => _instance = instance;

		protected virtual void SetupRecipes()
		{
		}

		/// <summary>
		///		Finishes the setup of the recipes, and creates the generators.
		/// </summary>
		public void CreateGenerators()
		{
			SetupRecipes();

			_modifierInfos = new ModifierInfo[_recipes.Count + _manualGenerators.Count];
			_tags = new TagType[_recipes.Count + _manualGenerators.Count];
			foreach (var generator in _manualGenerators.Values)
			{
				_modifierGenerators.Add(generator.Name, generator);
				_modifierInfos[generator.Id] = new ModifierInfo(generator.Id, generator.Name, generator.DisplayName,
					generator.Description);
				_tags[generator.Id] = generator.Tag;
			}

			foreach (var recipe in _recipes.Values)
			{
				_modifierGenerators.Add(recipe.Name, recipe.CreateModifierGenerator());
				_modifierInfos[recipe.Id] = recipe.CreateModifierInfo();
				_tags[recipe.Id] = recipe.GetTag();
			}

			GeneratorCount = _modifierGenerators.Count;
#if DEBUG && !MODIBUFF_PROFILE
			Logger.Log($"[ModiBuff] Loaded {GeneratorCount} modifier generators.");
#endif
		}

		public ModifierInfo GetModifierInfo(int id)
		{
			if (id < 0 || id >= _modifierInfos.Length)
			{
				Logger.LogError($"[ModiBuff] Modifier with id {id} does not exist.");
				return null;
			}

			return _modifierInfos[id];
		}

		public static ref readonly TagType GetTag(int id) => ref _instance._tags[id];

		public IModifierGenerator GetGenerator(string name) => _modifierGenerators[name];

		public IModifierGenerator[] GetGenerators() => _modifierGenerators.Values.ToArray();

		public ModifierRecipe Add(string name, string displayName = "", string description = "")
		{
			if (_recipes.TryGetValue(name, out var localRecipe))
			{
#if DEBUG && !MODIBUFF_PROFILE
				Logger.LogError($"[ModiBuff] Modifier recipe with name {name} already exists");
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

		public void Add(string name, string displayName, string description,
			in ModifierGeneratorFunc createFunc, TagType tag = TagType.Default)
		{
			if (_recipes.ContainsKey(name))
			{
#if DEBUG && !MODIBUFF_PROFILE
				Logger.LogError($"[ModiBuff] Modifier recipe with name {name} already exists");
#endif
				return;
			}

			if (_manualGenerators.ContainsKey(name))
			{
#if DEBUG && !MODIBUFF_PROFILE
				Logger.LogError($"[ModiBuff] Modifier generator with name {name} already exists");
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

			var modifierGenerator = new ManualModifierGenerator(id, name, displayName, description,
				in createFunc, tag);
			_manualGenerators.Add(name, modifierGenerator);
		}

		/// <summary>
		///		Special case when we want to use applier/conditional modifiers and want to set them up in order.
		///		This can also be used to have the first original modifier take the first available id.
		/// </summary>
		public void Register(params string[] names)
		{
			foreach (string name in names)
			{
				if (_registeredNames.Exists(d => d.Name == name))
				{
#if DEBUG && !MODIBUFF_PROFILE
					Logger.LogError($"[ModiBuff] Modifier with id {name} already exists");
#endif
					continue;
				}

				_registeredNames.Add(new RegisterData(name, _idManager.GetFreeId(name)));
			}
		}

		public void Clear()
		{
			_recipes.Clear();
			Array.Clear(_tags, 0, _tags.Length);
			_manualGenerators.Clear();
			_modifierGenerators.Clear();
			_registeredNames.Clear();
		}

		public SaveData SaveState()
		{
			var recipesSaveData = new ModifierRecipe.SaveData[_recipes.Count];
			int i = 0;
			foreach (var recipe in _recipes.Values)
				recipesSaveData[i++] = recipe.SaveState();

			return new SaveData(recipesSaveData);
		}

		public void TempRegisterEffects(Func<float, IEffect>[] types) => RegisterEffects = types;

		public static Func<float, IEffect>[] RegisterEffects;
		public static Dictionary<Type, int> IdToEffect;

		public void LoadState(SaveData saveData)
		{
			foreach (var recipeSaveData in saveData.RecipesSaveData)
			{
				ModifierRecipe recipe = null;
				foreach (var instruction in recipeSaveData.Instructions)
				{
					if (instruction.InstructionId == ModifierRecipe.SaveInstruction.Init.Id)
					{
						recipe = Add(instruction.Values.GetDataFromJsonObject<string>(),
							"" /*init.DisplayName, init.Description*/);
						break;
					}
				}

				if (recipe == null)
				{
					//TODO Identification
					Logger.LogError("[ModiBuff] Failed to load recipe, it's missing an Init instruction?");
					continue;
				}

				recipe.LoadState(recipeSaveData);
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

		public readonly struct SaveData
		{
			public readonly ModifierRecipe.SaveData[] RecipesSaveData;

#if MODIBUFF_SYSTEM_TEXT_JSON
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(ModifierRecipe.SaveData[] recipesSaveData) => RecipesSaveData = recipesSaveData;
		}
	}
}