using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ModifierController : IRemoveModifier
	{
		private readonly Modifier[] _modifiers;
		private readonly List<int> _modifierIndexes;

		private readonly List<int> _modifierAttackAppliers;
		private readonly List<int> _modifierCastAppliers;
		private readonly Dictionary<int, ModifierCheck> _modifierChecksAppliers;

		private readonly List<Modifier> _modifiersToRemove;

		public ModifierController()
		{
			_modifierIndexes = new List<int>(5);
			_modifiers = new Modifier[ModifierRecipes.RecipesCount];
			_modifierAttackAppliers = new List<int>(5);
			_modifierCastAppliers = new List<int>(5);
			_modifierChecksAppliers = new Dictionary<int, ModifierCheck>(5);

			_modifiersToRemove = new List<Modifier>(5);
		}

		public void Update(in float delta)
		{
			int length = _modifierIndexes.Count;
			for (int i = 0; i < length; i++)
				_modifiers[_modifierIndexes[i]].Update(delta);

			if (_modifierChecksAppliers.Count > 0)
				foreach (var check in _modifierChecksAppliers.Values)
					check.Update(delta);

			int removeCount = _modifiersToRemove.Count;
			if (removeCount == 0)
				return;

			//Debug.Log("ModifiersRemove: " + _modifiersToRemoveNew.Count);
			for (int i = 0; i < removeCount; i++)
				Remove(_modifiersToRemove[i]);

			_modifiersToRemove.Clear();
		}

		public IReadOnlyCollection<ModifierCheck> GetApplierCheckModifiers() => _modifierChecksAppliers.Values;
		public IReadOnlyList<int> GetApplierAttackModifiers() => _modifierAttackAppliers;
		public IReadOnlyList<int> GetApplierCastModifiers() => _modifierCastAppliers;

		public bool TryAdd(ModifierAddReference addReference, IUnit self, IUnit acter)
		{
			switch (addReference.ApplierType)
			{
				case ApplierType.None:
					return TryAdd(addReference.Id, self, acter);
				case ApplierType.Cast:
					return TryAddApplier(addReference.Recipe, addReference.ApplierType);
				case ApplierType.Attack:
					return TryAddApplier(addReference.Recipe, addReference.ApplierType);
				default:
					throw new ArgumentOutOfRangeException();
			}

			return true;
		}

		//TODO do appliers make sense? Should we just store the id, what kind of state do appliers have?
		public bool TryAdd(int id, IUnit target, IUnit acter)
		{
			//TODO We should check if the target is legal here?
			Add(id, target, acter);

			return true;
		}

		public bool TryAddApplier(IModifierRecipe recipe, ApplierType applierType = ApplierType.None)
		{
			if (!recipe.HasApplyChecks)
			{
				switch (applierType)
				{
					case ApplierType.Cast:
						if (_modifierCastAppliers.Contains(recipe.Id))
							return false;
						_modifierCastAppliers.Add(recipe.Id);
						return true;
					case ApplierType.Attack:
						if (_modifierAttackAppliers.Contains(recipe.Id))
							return false;
						_modifierAttackAppliers.Add(recipe.Id);
						return true;
				}

				return false;
			}

			if (_modifierChecksAppliers.ContainsKey(recipe.Id))
				return false;

			_modifierChecksAppliers.Add(recipe.Id, recipe.CreateApplyCheck());
			return true;
		}

		private Modifier Add(int id, IUnit target, IUnit acter)
		{
			var existingModifier = _modifiers[id];
			if (existingModifier != null)
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

			//TODO Do we want to save the sender of the original modifier? Ex. for thorns. Because owner is always the owner of the modifier instance
			modifier.SetTargets(target, acter);

			_modifiers[id] = modifier;
			_modifierIndexes.Add(id);
			modifier.Init();
			modifier.Refresh();
			modifier.Stack();
			return modifier;
		}

		public bool Contains(int id) => _modifiers[id] != null;

		public void PrepareRemove(Modifier modifier)
		{
			_modifiersToRemove.Add(modifier);
		}

		public void PrepareRemove(int id)
		{
			_modifiersToRemove.Add(_modifiers[id]);
		}

		public void Remove(int id)
		{
			Remove(_modifiers[id]);
		}

		private void Remove(Modifier modifier)
		{
			//Debug.Log("Removing modifier: " + modifier.Id);
			_modifiers[modifier.Id] = null;
			_modifierIndexes.Remove(modifier.Id);
			ModifierPool.Instance.Return(modifier);
		}
	}
}