using System.Collections.Generic;
using UnityEngine;

namespace ModifierLibraryLite.Core
{
	public sealed class ModifierController
	{
		//TODO Array mapping?
		private readonly IDictionary<string, Modifier> _modifiers;

		//private readonly HashSet<ModifierRecipe> _modifierRecipeAppliers;
		private readonly List<ModifierCheck> _modifierChecksAppliers;

		private readonly List<string> _modifiersToRemove;

		private static ModifierPool _modifierPool;

		public ModifierController()
		{
			_modifiers = new Dictionary<string, Modifier>();
			//_modifierRecipeAppliers = new HashSet<ModifierRecipe>(5);
			_modifierChecksAppliers = new List<ModifierCheck>(5);

			_modifiersToRemove = new List<string>(5);
		}

		public void Update(in float delta)
		{
			//int length = _modifiers.Count;
			//TODO Array for loop mapping
			foreach (var modifier in _modifiers.Values)
			{
				modifier.Update(delta);

				if (modifier.ToRemove)
					_modifiersToRemove.Add(modifier.Id);
			}

			for (int i = 0; i < _modifiersToRemove.Count; i++)
				_modifiers.Remove(_modifiersToRemove[i]);
		}

		public IReadOnlyCollection<ModifierCheck> GetApplierModifiers()
		{
			return _modifierChecksAppliers;
		}

		//TODO do appliers make sense? Should we just store the id, what kind of state do appliers have?
		public (bool Success, Modifier Modifier) TryAdd(string id, IUnit owner, IUnit target, IUnit sender = null)
		{
			//TODO We should call the original modifier's check component here or before

			return (true, Add(id, owner, target, sender));
		}

		public bool TryAddApplier(ModifierRecipe recipe)
		{
			//TODO If no checks. Only add an id?

			//TODO
			if (_modifierChecksAppliers.Contains(recipe.CreateCheck()))
				return false;

			_modifierChecksAppliers.Add(recipe.CreateCheck());
			return true;
		}

		public bool TryAddAppliers(ModifierRecipe[] recipes)
		{
			bool success = true;
			foreach (var recipe in recipes)
			{
				if (!TryAddApplier(recipe))
					success = false;
			}

			return success;
		}

		private Modifier Add(string id, IUnit owner, IUnit target, IUnit sender = null)
		{
			if (_modifiers.TryGetValue(id, out var existingModifier))
			{
				//Debug.Log("Modifier already exists");
				//TODO should we update the modifier targets when init/refreshing/stacking?
				existingModifier.Init();
				existingModifier.Refresh();
				existingModifier.Stack();
				return existingModifier;
			}

			//Debug.Log("Adding new modifier");
			var modifier = ModifierPool.Instance.Rent(id);
			//var modifier = recipe.Create();

			//TODO Do we want to save the sender of the original modifier? Ex. for thorns. Because owner is always the owner of the modifier instance
			modifier.SetTargets(target, owner, sender);

			_modifiers.Add(modifier.Id, modifier);
			modifier.Init();
			modifier.Refresh();
			modifier.Stack();
			return modifier;
		}

		public bool Contains(ModifierRecipe recipe) => Contains(recipe.Id);
		public bool Contains(Modifier modifier) => Contains(modifier.Id);
		public bool Contains(string id) => _modifiers.ContainsKey(id);
	}
}