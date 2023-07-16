using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ModifierController
	{
		private readonly Modifier[] _modifiers;
		private readonly List<int> _modifierIndexes;

		private readonly List<int> _modifierAttackAppliers;
		private readonly List<int> _modifierCastAppliers;
		private readonly Dictionary<int, ModifierCheck> _modifierChecksAppliers;

		private readonly List<int> _modifiersToRemove;

		public ModifierController()
		{
			_modifierIndexes = new List<int>(5);
			_modifiers = new Modifier[ModifierRecipesBase.RecipesCount];
			_modifierAttackAppliers = new List<int>(5);
			_modifierCastAppliers = new List<int>(5);
			_modifierChecksAppliers = new Dictionary<int, ModifierCheck>(5);

			_modifiersToRemove = new List<int>(5);
		}

		public void Update(float delta)
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

			for (int i = 0; i < removeCount; i++)
				Remove(_modifiersToRemove[i]);

			_modifiersToRemove.Clear();
		}

		public IReadOnlyCollection<ModifierCheck> GetApplierCheckModifiers() => _modifierChecksAppliers.Values;
		public IReadOnlyList<int> GetApplierAttackModifiers() => _modifierAttackAppliers;
		public IReadOnlyList<int> GetApplierCastModifiers() => _modifierCastAppliers;
		public bool GetApplierCastModifier(int id) => _modifierCastAppliers.Contains(id);

		public bool TryAdd(ModifierAddReference addReference, IUnit self, IUnit source)
		{
			return TryAdd(addReference.Id, addReference.HasApplyChecks, addReference.ApplierType, self, source);
		}

		//TODO Refactor, make it easier to add appliers through API
		private bool TryAdd(int id, bool hasApplyChecks, ApplierType applierType, IUnit self, IUnit source)
		{
			switch (applierType)
			{
				case ApplierType.None:
					return TryAdd(id, self, source);
				case ApplierType.Cast:
				case ApplierType.Attack:
					return TryAddApplier(id, hasApplyChecks, applierType);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		//TODO do appliers make sense? Should we just store the id, what kind of state do appliers have?
		public bool TryAdd(int id, IUnit target, IUnit source)
		{
			//TODO We should check if the target is legal here?
			Add(id, target, source);

			return true;
		}

		public bool TryAddApplier(int id, bool hasApplyChecks, ApplierType applierType = ApplierType.None)
		{
			if (!hasApplyChecks)
			{
				switch (applierType)
				{
					case ApplierType.Cast:
						if (_modifierCastAppliers.Contains(id))
							return false;
						_modifierCastAppliers.Add(id);
						return true;
					case ApplierType.Attack:
						if (_modifierAttackAppliers.Contains(id))
							return false;
						_modifierAttackAppliers.Add(id);
						return true;
				}

				return false;
			}

			if (_modifierChecksAppliers.ContainsKey(id))
				return false;

			_modifierChecksAppliers.Add(id, ModifierPool.Instance.RentModifierCheck(id));
			return true;
		}

		private Modifier Add(int id, IUnit target, IUnit source)
		{
			var existingModifier = _modifiers[id];
			if (existingModifier != null)
			{
				//Debug.Log("Modifier already exists");
				//TODO should we update the modifier targets when init/refreshing/stacking?
				existingModifier.UpdateSource(source);
				existingModifier.Init();
				existingModifier.Refresh();
				existingModifier.Stack();
				return existingModifier;
			}

			//Debug.Log("Adding new modifier");
			var modifier = ModifierPool.Instance.Rent(id);

			//TODO Do we want to save the sender of the original modifier? Ex. for thorns. Because owner is always the owner of the modifier instance
			modifier.SetTarget(new SingleTargetComponent(target, source));

			_modifiers[id] = modifier;
			_modifierIndexes.Add(id);
			modifier.Init();
			modifier.Refresh();
			modifier.Stack();
			return modifier;
		}

		public bool Contains(int id) => _modifiers[id] != null;

		public void PrepareRemove(int id) => _modifiersToRemove.Add(id);

		internal void Remove(int id)
		{
			var modifier = _modifiers[id];
			//Debug.Log("Removing modifier: " + modifier.Id);
			_modifiers[id] = null;
			_modifierIndexes.Remove(id);
			ModifierPool.Instance.Return(modifier);
		}

		/// <summary>
		///		Returns all modifiers back to the pool
		/// </summary>
		public void Clear()
		{
			for (int i = 0; i < _modifierIndexes.Count; i++)
				Remove(_modifierIndexes[i]);

			foreach (var check in _modifierChecksAppliers.Values)
				ModifierPool.Instance.ReturnCheck(check);

			//Clear the rest, if the unit will be reused/pooled. Otherwise this is not needed
			_modifierIndexes.Clear();
			_modifierAttackAppliers.Clear();
			_modifierCastAppliers.Clear();
			_modifierChecksAppliers.Clear();
			_modifiersToRemove.Clear();
		}
	}
}