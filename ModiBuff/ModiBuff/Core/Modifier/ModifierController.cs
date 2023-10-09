using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	/// <summary>
	///		ModifierController is the class that handles all modifiers for a unit
	///		It allows us to add and update modifier by id
	/// </summary>
	/// <remarks>This version supports multiple modifiers of the same type</remarks>
	public sealed class ModifierController
	{
		private readonly IUnit _owner;

		//A dict with multikey can be used, but we run into problems with modifiers that don't use instance stacking
		private Modifier[] _modifiers;
		private readonly int[] _modifierIndexes;
		private int _modifiersTop;

		private readonly List<int> _modifierAttackAppliers;

		//TODO Will there be cast modifier without any appliers?
		private readonly List<int> _modifierCastAppliers;

		private readonly Dictionary<int, ModifierCheck> _modifierCastChecksAppliers;
		private readonly Dictionary<int, ModifierCheck> _modifierAttackChecksAppliers;

		private readonly List<ModifierReference> _modifiersToRemove;

		public ModifierController(IUnit owner)
		{
			_owner = owner;

			_modifiers = new Modifier[Config.ModifierArraySize];
			_modifierIndexes = new int[ModifierRecipes.GeneratorCount];
			for (int i = 0; i < _modifierIndexes.Length; i++)
				_modifierIndexes[i] = -1;

			_modifierAttackAppliers = new List<int>(Config.AttackApplierSize);
			_modifierCastAppliers = new List<int>(Config.CastApplierSize);
			_modifierCastChecksAppliers = new Dictionary<int, ModifierCheck>(Config.CastCheckApplierSize);
			_modifierAttackChecksAppliers = new Dictionary<int, ModifierCheck>(Config.AttackCheckApplierSize);

			_modifiersToRemove = new List<ModifierReference>(Config.ModifierRemoveSize);
		}

		public void Update(float delta)
		{
			int modifiersTop = _modifiersTop;
			for (int i = 0; i < modifiersTop; i++)
				_modifiers[i].Update(delta);

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

		/// <summary>
		///		Gets timer reference, used to update UI/UX
		/// </summary>
		/// <param name="timeComponentNumber">Which timer should be returned, first = 0</param>
		public ITimeReference GetTimer<TTimeComponent>(int id, int genId = 0, int timeComponentNumber = 0)
			where TTimeComponent : ITimeComponent
		{
			return GetModifier(id, genId)?.GetTimer<TTimeComponent>(timeComponentNumber);
		}

		/// <summary>
		///		Get stack reference, used to update UI/UX
		/// </summary>
		public IStackReference GetStackReference(int id, int genId = 0)
		{
			return GetModifier(id, genId)?.GetStackReference();
		}

		/// <summary>
		///		Gets state from effect, used to display values in UI
		/// </summary>
		/// <param name="stateNumber">Which state should be returned, 0 = first</param>
		public TData GetState<TData>(int id, int genId = 0, int stateNumber = 0) where TData : struct
		{
			var modifier = GetModifier(id, genId);
			if (modifier != null)
				return modifier.GetState<TData>(stateNumber);

			Logger.LogWarning($"Couldn't find state info, {typeof(TData)} at number {stateNumber}, " +
			                  $"id: {id}, genId: {genId}");
			return default;
		}

		public bool TryAdd(ModifierAddReference addReference) => TryAdd(addReference, _owner);

		public bool TryAdd(ModifierAddReference addReference, IUnit target)
		{
			if (addReference.IsApplierType)
				return TryAddApplier(addReference.Id, addReference.HasApplyChecks, addReference.ApplierType);

			Add(addReference.Id, target, _owner);
			return true;
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
#if DEBUG && !MODIBUFF_PROFILE
					Logger.LogError("Unknown applier type: " + applierType);
#endif
					return false;
			}
		}

		public void TryApplyAttackNonCheckModifiers(IEnumerable<int> modifierIds, IUnit target, IModifierOwner source)
		{
			foreach (int id in modifierIds)
				Add(id, target, source);
		}

		public void TryApplyAttackCheckModifiers(IEnumerable<ModifierCheck> modifierChecks, IUnit target,
			IModifierOwner source)
		{
			foreach (var check in modifierChecks)
				if (check.Check(source))
					Add(check.Id, target, source);
		}

		public int Add(int id, IUnit target, IUnit source)
		{
			ref readonly var tag = ref ModifierRecipes.GetTag(id);

			if (!tag.HasTag(TagType.IsInstanceStackable) && _modifierIndexes[id] != -1)
			{
				var existingModifier = _modifiers[_modifierIndexes[id]];
				//TODO should we update the modifier targets when init/refreshing/stacking?
				existingModifier.UpdateSource(source);
				if ((tag & TagType.IsInit) != 0)
					existingModifier.Init();
				if ((tag & TagType.IsRefresh) != 0)
					existingModifier.Refresh();
				if ((tag & TagType.IsStack) != 0)
					existingModifier.Stack();

				return existingModifier.GenId;
			}

			if (_modifiersTop == _modifiers.Length)
				Array.Resize(ref _modifiers, _modifiers.Length << 1);

			var modifier = ModifierPool.Instance.Rent(id);

			//TODO Do we want to save the sender of the original modifier? Ex. for thorns. Because owner is always the owner of the modifier instance
			modifier.UpdateSingleTargetSource(target, source);

			if (!tag.HasTag(TagType.IsInstanceStackable))
				_modifierIndexes[id] = _modifiersTop;
			_modifiers[_modifiersTop++] = modifier;
			if (tag.HasTag(TagType.IsInit))
				modifier.Init();
			if (tag.HasTag(TagType.IsStack))
				modifier.Stack();

			return modifier.GenId;
		}

		public bool Contains(int id)
		{
			if (!ModifierRecipes.GetTag(id).HasTag(TagType.IsInstanceStackable))
				return _modifierIndexes[id] != -1;

			for (int i = 0; i < _modifiersTop; i++)
				if (_modifiers[i].Id == id)
					return true;

			//TODO GenId if we want to check for specific modifiers

			return false;
		}

		public bool ContainsApplier(int id) =>
			_modifierCastAppliers.Contains(id) || _modifierCastChecksAppliers.ContainsKey(id);

		public void PrepareRemove(int id, int genId)
		{
			_modifiersToRemove.Add(new ModifierReference(id, genId));
		}

		public void ModifierAction(int id, int genId, ModifierAction action)
		{
			ref readonly var tag = ref ModifierRecipes.GetTag(id);
			var modifier = GetModifier(id, genId);

			if (modifier == null)
			{
#if DEBUG && !MODIBUFF_PROFILE
				Logger.LogError($"Tried to call modifier action {action} on a modifier that " +
				                $"doesn't exist, id: {id}, genId: {genId}");
#endif
				return;
			}

			switch (action)
			{
				case Core.ModifierAction.Refresh:
#if DEBUG && !MODIBUFF_PROFILE
					if (!tag.HasTag(TagType.IsRefresh))
						Logger.LogWarning("ModifierAction: Refresh was called on a modifier that " +
						                  "doesn't have a refresh flag set");
#endif
					modifier.Refresh();
					break;
				case Core.ModifierAction.ResetStacks:
#if DEBUG && !MODIBUFF_PROFILE
					if (!tag.HasTag(TagType.IsStack))
						Logger.LogWarning("ModifierAction: ResetStacks was called on a modifier " +
						                  "that doesn't have a stack flag set");
#endif
					modifier.ResetStacks();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(action), action, "Invalid modifier action");
			}
		}

		public void Remove(in ModifierReference modifierReference)
		{
			if (!ModifierRecipes.GetTag(modifierReference.Id).HasTag(TagType.IsInstanceStackable))
			{
#if DEBUG && !MODIBUFF_PROFILE
				if (_modifierIndexes[modifierReference.Id] == -1)
				{
					Logger.LogError("Tried to remove a modifier that doesn't exist on entity, id: " +
					                $"{modifierReference.Id}, genId: {modifierReference.GenId}");
					return;
				}
#endif
				var modifier = _modifiers[_modifierIndexes[modifierReference.Id]];
				ModifierPool.Instance.Return(modifier);
				_modifiers[_modifierIndexes[modifierReference.Id]] = _modifiers[--_modifiersTop];
				_modifiers[_modifiersTop] = null;
				_modifierIndexes[modifierReference.Id] = -1;
				return;
			}

			for (int i = 0; i < _modifiersTop; i++)
			{
				var modifier = _modifiers[i];
				if (modifier.Id == modifierReference.Id && modifier.GenId == modifierReference.GenId)
				{
					ModifierPool.Instance.Return(modifier);
					_modifiers[i] = _modifiers[--_modifiersTop]; //TODO This switching might cause some order issues
					_modifiers[_modifiersTop] = null;
					break;
				}
			}
		}

		/// <summary>
		///		Returns all modifiers back to the pool
		/// </summary>
		public void Clear()
		{
			for (int i = 0; i < _modifiersTop; i++)
			{
				ModifierPool.Instance.Return(_modifiers[i]);
				_modifiers[i] = null;
			}

			for (int i = 0; i < _modifierIndexes.Length; i++)
				_modifierIndexes[i] = -1;

			_modifiersTop = 0;

			foreach (var check in _modifierCastChecksAppliers.Values)
				ModifierPool.Instance.ReturnCheck(check);

			foreach (var check in _modifierAttackChecksAppliers.Values)
				ModifierPool.Instance.ReturnCheck(check);

			//Clear the rest, if the unit will be reused/pooled. Otherwise this is not needed
			_modifierAttackAppliers.Clear();
			_modifierCastAppliers.Clear();
			_modifierCastChecksAppliers.Clear();
			_modifierAttackChecksAppliers.Clear();
			_modifiersToRemove.Clear();
		}

		private Modifier GetModifier(int id, int genId)
		{
			if (!ModifierRecipes.GetTag(id).HasTag(TagType.IsInstanceStackable))
			{
				int modifierIndex = _modifierIndexes[id];
				return modifierIndex != -1 ? _modifiers[modifierIndex] : null;
			}

			for (int i = 0; i < _modifiersTop; i++)
			{
				var modifier = _modifiers[i];
				if (modifier.Id == id && modifier.GenId == genId)
					return modifier;
			}

			return null;
		}
	}
}