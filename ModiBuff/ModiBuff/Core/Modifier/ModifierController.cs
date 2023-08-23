using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ModifierController
	{
		private readonly IUnit _owner;

		private readonly Modifier[] _modifiers;
		private readonly List<int> _modifierIndexes;

		private readonly List<int> _modifierAttackAppliers;
		private readonly List<int> _modifierCastAppliers; //TODO Will there be cast modifier without any appliers?
		private readonly Dictionary<int, ModifierCheck> _modifierCastChecksAppliers;
		private readonly Dictionary<int, ModifierCheck> _modifierAttackChecksAppliers;

		private readonly List<int> _modifiersToRemove;

		public ModifierController(IUnit owner)
		{
			_owner = owner;

			_modifierIndexes = new List<int>(5);
			_modifiers = new Modifier[ModifierRecipes.RecipesCount];
			_modifierAttackAppliers = new List<int>(5);
			_modifierCastAppliers = new List<int>(5);
			_modifierCastChecksAppliers = new Dictionary<int, ModifierCheck>(5);
			_modifierAttackChecksAppliers = new Dictionary<int, ModifierCheck>(5);

			_modifiersToRemove = new List<int>(5);
		}

		public void Update(float delta)
		{
			int length = _modifierIndexes.Count;
			for (int i = 0; i < length; i++)
				_modifiers[_modifierIndexes[i]].Update(delta);

			if (_modifierCastChecksAppliers.Count > 0)
				foreach (var check in _modifierCastChecksAppliers.Values)
					check.Update(delta);

			if (_modifierAttackChecksAppliers.Count > 0)
				foreach (var check in _modifierAttackChecksAppliers.Values)
					check.Update(delta);

			int removeCount = _modifiersToRemove.Count;
			if (removeCount == 0)
				return;

			for (int i = 0; i < removeCount; i++)
				Remove(_modifiersToRemove[i]);

			_modifiersToRemove.Clear();
		}

		public ICollection<ModifierCheck> GetApplierCastCheckModifiers() => _modifierCastChecksAppliers.Values;
		public ICollection<ModifierCheck> GetApplierAttackCheckModifiers() => _modifierAttackChecksAppliers.Values;

		public IReadOnlyList<int> GetApplierAttackModifierIds() => _modifierAttackAppliers;
		public IReadOnlyList<int> GetApplierCastModifierIds() => _modifierCastAppliers;

		public bool TryAdd(ModifierAddReference addReference)
		{
			return TryAdd(addReference.Id, addReference.HasApplyChecks, addReference.ApplierType);
		}

		//TODO Refactor, make it easier to add appliers through API
		private bool TryAdd(int id, bool hasApplyChecks, ApplierType applierType)
		{
			switch (applierType)
			{
				case ApplierType.None:
					Logger.LogError("Trying to add a non applier modifier with apply checks");
					return false;
				case ApplierType.Cast:
				case ApplierType.Attack:
					return TryAddApplier(id, hasApplyChecks, applierType);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		//TODO do appliers make sense? Should we just store the id, what kind of state do appliers have?

		/// <summary>
		///		Only triggers the check, does not trigger the modifiers effect. Used when modifiers 
		/// </summary>
		public bool TryCastCheck(int id)
		{
			return _modifierCastChecksAppliers.TryGetValue(id, out var check) && check.Check(_owner);
		}

		/// <summary>
		///		Skips the check part for check modifiers, use this ONLY in case you're also using <see cref="TryCastCheck"/>
		/// </summary>
		public bool TryCastModifierWithoutCheck(int id, IUnit target, IModifierOwner source) //TODO Rename
		{
			if (source.ModifierController._modifierCastChecksAppliers.ContainsKey(id))
			{
				Add(id, target, source);
				return true;
			}

			return false;
		}

		/// <summary>
		///		Checks if we can cast the modifier, triggers the check if it exists
		/// </summary>
		public bool CanCastModifier(int id)
		{
			if (_modifierCastAppliers.Contains(id))
				return true;

			return _modifierCastChecksAppliers.TryGetValue(id, out var check) && check.Check(_owner);
		}

		public bool CanUseAttackModifier(int id)
		{
			if (_modifierAttackAppliers.Contains(id))
				return true;

			return _modifierAttackChecksAppliers.TryGetValue(id, out var check) && check.Check(_owner);
		}

		public bool TryAddApplier(int id, bool hasApplyChecks, ApplierType applierType)
		{
			switch (applierType)
			{
				case ApplierType.Cast when hasApplyChecks:
				{
					if (_modifierCastChecksAppliers.ContainsKey(id))
						return false;

					_modifierCastChecksAppliers.Add(id, ModifierPool.Instance.RentModifierCheck(id));
					return true;
				}
				case ApplierType.Cast:
				{
					if (_modifierCastAppliers.Contains(id))
						return false;

					_modifierCastAppliers.Add(id);
					return true;
				}
				case ApplierType.Attack when hasApplyChecks:
				{
					if (_modifierAttackChecksAppliers.ContainsKey(id))
						return false;

					_modifierAttackChecksAppliers.Add(id, ModifierPool.Instance.RentModifierCheck(id));
					return true;
				}
				case ApplierType.Attack:
				{
					if (_modifierAttackAppliers.Contains(id))
						return false;

					_modifierAttackAppliers.Add(id);
					return true;
				}
				default:
					Logger.LogError("Unknown applier type: " + applierType);
					return false;
			}
		}

		public void TryApplyAttackNonCheckModifiers(IEnumerable<int> modifierIds, IUnit target, IModifierOwner source)
		{
			foreach (int id in modifierIds)
				Add(id, target, source);
		}

		public void TryApplyAttackCheckModifiers(IEnumerable<ModifierCheck> modifierChecks, IUnit target, IModifierOwner source)
		{
			foreach (var check in modifierChecks)
				if (check.Check(source))
					Add(check.Id, target, source);
		}

		public void Add(int id, IUnit target, IUnit source)
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
				return;
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
		}

		public bool Contains(int id)
		{
			return _modifiers[id] != null;
		}

		public void PrepareRemove(int id)
		{
			_modifiersToRemove.Add(id);
		}

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

			foreach (var check in _modifierCastChecksAppliers.Values)
				ModifierPool.Instance.ReturnCheck(check);

			foreach (var check in _modifierAttackChecksAppliers.Values)
				ModifierPool.Instance.ReturnCheck(check);

			//Clear the rest, if the unit will be reused/pooled. Otherwise this is not needed
			_modifierIndexes.Clear();
			_modifierAttackAppliers.Clear();
			_modifierCastAppliers.Clear();
			_modifierCastChecksAppliers.Clear();
			_modifierAttackChecksAppliers.Clear();
			_modifiersToRemove.Clear();
		}
	}
}