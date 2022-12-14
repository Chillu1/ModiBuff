using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

namespace ModifierLibraryLite.Core
{
	public sealed class ModifierController : IRemoveModifier
	{
		//TODO Array mapping?
		private readonly Dictionary<int, Modifier> _modifiers;

		private readonly List<ModifierRecipe> _modifierAppliers;
		private readonly Dictionary<int, ModifierCheck> _modifierChecksAppliers;

		private readonly List<Modifier> _modifiersToRemoveNew;

		public ModifierController()
		{
			_modifiers = new Dictionary<int, Modifier>();
			_modifierAppliers = new List<ModifierRecipe>(5);
			_modifierChecksAppliers = new Dictionary<int, ModifierCheck>(5);

			_modifiersToRemoveNew = new List<Modifier>(5);
		}

		public void Update(in float delta)
		{
			//int length = _modifiers.Count;
			//TODO Array for loop mapping
			foreach (var modifier in _modifiers.Values)
				modifier.Update(delta);

			foreach (var check in _modifierChecksAppliers.Values)
				check.Update(delta);

			int removeCount = _modifiersToRemoveNew.Count;
			if (removeCount == 0)
				return;

			//Debug.Log("ModifiersRemove: " + _modifiersToRemoveNew.Count);
			for (int i = 0; i < removeCount; i++)
				Remove(_modifiersToRemoveNew[i]);

			_modifiersToRemoveNew.Clear();
		}

		public IReadOnlyCollection<ModifierCheck> GetApplierCheckModifiers() => _modifierChecksAppliers.Values;
		public IReadOnlyList<ModifierRecipe> GetApplierModifiers() => _modifierAppliers;

		//TODO do appliers make sense? Should we just store the id, what kind of state do appliers have?
		public bool TryAdd(int id, IUnit owner, IUnit target, IUnit sender = null)
		{
			//TODO We should check if the target is legal here?
			Add(id, owner, target, sender);

			return true;
		}

		public bool TryAddApplier(ModifierRecipe recipe)
		{
			if (!recipe.HasChecks)
			{
				if (_modifierAppliers.Contains(recipe))
					return false;

				_modifierAppliers.Add(recipe);
				return true;
			}

			if (_modifierChecksAppliers.ContainsKey(recipe.Id))
				return false;

			_modifierChecksAppliers.Add(recipe.Id, recipe.CreateCheck());
			return true;
		}

		public bool TryAddAppliers(ModifierRecipe[] recipes)
		{
			bool success = true;
			for (int i = 0; i < recipes.Length; i++)
			{
				if (!TryAddApplier(recipes[i]))
					success = false;
			}

			return success;
		}

		private Modifier Add(int id, IUnit owner, IUnit target, IUnit sender = null)
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

			modifier.SetupModifierRemove(this);
			//TODO Do we want to save the sender of the original modifier? Ex. for thorns. Because owner is always the owner of the modifier instance
			modifier.SetTargets(target, owner, sender);

			_modifiers.Add(modifier.Id, modifier);
			modifier.Init();
			modifier.Refresh();
			modifier.Stack();
			return modifier;
		}

		public bool Contains(int id) => _modifiers.ContainsKey(id);

		public void PrepareRemove(Modifier modifier)
		{
			_modifiersToRemoveNew.Add(modifier);
		}

		public void Remove(int id)
		{
			Remove(_modifiers[id]);
		}

		private void Remove(Modifier modifier)
		{
			//Debug.Log("Removing modifier: " + modifier.Id);
			_modifiers.Remove(modifier.Id);
			ModifierPool.Instance.Return(modifier);
		}
	}
}