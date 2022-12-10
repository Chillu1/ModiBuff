using System.Collections.Generic;
using UnityEngine;

namespace ModifierLibraryLite.Core
{
	public sealed class ModifierController
	{
		//TODO Array mapping?
		private readonly IDictionary<string, Modifier> _modifiers;
		private readonly HashSet<ModifierRecipe> _modifierRecipeAppliers;

		private readonly List<string> _modifiersToRemove;

		public ModifierController()
		{
			_modifiers = new Dictionary<string, Modifier>();
			_modifierRecipeAppliers = new HashSet<ModifierRecipe>(5);

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

		public IReadOnlyCollection<ModifierRecipe> GetApplierModifiers()
		{
			return _modifierRecipeAppliers;
		}

		//TODO do appliers make sense? Should we just store the id, what kind of state do appliers have?
		public (bool Success, Modifier Modifier) TryAdd(Modifier modifier)
		{
			//TODO We should call the original modifier's check component here or before
			return (true, Add(modifier));
		}

		public bool TryAddApplier(ModifierRecipe recipe)
		{
			if (_modifierRecipeAppliers.Contains(recipe))
				return false;

			_modifierRecipeAppliers.Add(recipe);
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

		private Modifier Add(Modifier modifier)
		{
			//TODO We should call the original modifier's check component before

			if (_modifiers.TryGetValue(modifier.Id, out var existingModifier))
			{
				existingModifier.Init();
				existingModifier.Refresh();
				existingModifier.Stack();
				return existingModifier;
			}

			_modifiers.Add(modifier.Id, modifier);
			modifier.Init();
			modifier.Refresh();
			modifier.Stack();
			return modifier;
		}

		public bool Contains(Modifier modifier)
		{
			return _modifiers.ContainsKey(modifier.Id);
		}
	}
}