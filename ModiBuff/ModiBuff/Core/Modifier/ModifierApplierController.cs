using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ModifierApplierController
	{
		private readonly IUnit _owner;

		private readonly List<int> _modifierAttackAppliers;

		//TODO Will there be cast modifier without any appliers?
		private readonly List<int> _modifierCastAppliers;

		private readonly Dictionary<int, ModifierCheck> _modifierCastChecksAppliers;
		private readonly Dictionary<int, ModifierCheck> _modifierAttackChecksAppliers;

		private readonly List<int> _effectCasts;

		public ModifierApplierController(IUnit owner)
		{
			_owner = owner;

			_modifierAttackAppliers = new List<int>(Config.AttackApplierSize);
			_modifierCastAppliers = new List<int>(Config.CastApplierSize);
			_modifierCastChecksAppliers = new Dictionary<int, ModifierCheck>(Config.CastCheckApplierSize);
			_modifierAttackChecksAppliers = new Dictionary<int, ModifierCheck>(Config.AttackCheckApplierSize);

			_effectCasts = new List<int>(Config.EffectCastsSize);
		}

		public void Update(float delta)
		{
			foreach (var check in _modifierCastChecksAppliers.Values)
				check.Update(delta);

			foreach (var check in _modifierAttackChecksAppliers.Values)
				check.Update(delta);
		}

		public ICollection<ModifierCheck> GetApplierCastCheckModifiers() => _modifierCastChecksAppliers.Values;
		public ICollection<ModifierCheck> GetApplierAttackCheckModifiers() => _modifierAttackChecksAppliers.Values;

		public IReadOnlyList<int> GetApplierAttackModifierIds() => _modifierAttackAppliers;
		public IReadOnlyList<int> GetApplierCastModifierIds() => _modifierCastAppliers;

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

		public bool CanCastEffect(int id) => _effectCasts.Contains(id);

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
					{
						Logger.LogWarning("[ModiBuff] Tried to add a duplicate cast check applier, id: " + id);
						return false;
					}

					_modifierCastChecksAppliers.Add(id, ModifierPool.Instance.RentModifierCheck(id));
					return true;
				}
				case ApplierType.Cast:
				{
					if (_modifierCastAppliers.Contains(id))
					{
						Logger.LogWarning("[ModiBuff] Tried to add a duplicate cast applier, id: " + id);
						return false;
					}

					_modifierCastAppliers.Add(id);
					return true;
				}
				case ApplierType.Attack when hasApplyChecks:
				{
					if (_modifierAttackChecksAppliers.ContainsKey(id))
					{
						Logger.LogWarning("[ModiBuff] Tried to add a duplicate attack check applier, id: " + id);
						return false;
					}

					_modifierAttackChecksAppliers.Add(id, ModifierPool.Instance.RentModifierCheck(id));
					return true;
				}
				case ApplierType.Attack:
				{
					if (_modifierAttackAppliers.Contains(id))
					{
						Logger.LogWarning("[ModiBuff] Tried to add a duplicate attack applier, id: " + id);
						return false;
					}

					_modifierAttackAppliers.Add(id);
					return true;
				}
				default:
#if DEBUG && !MODIBUFF_PROFILE
					Logger.LogError("[ModiBuff] Unknown applier type: " + applierType);
#endif
					return false;
			}
		}

		public bool TryAddEffectApplier(int id)
		{
			if (_effectCasts.Contains(id))
			{
				Logger.LogWarning("[ModiBuff] Tried to add a duplicate effect applier, id: " + id);
				return false;
			}

			_effectCasts.Add(id);
			return true;
		}

		public bool ContainsApplier(int id) =>
			_modifierCastAppliers.Contains(id) || _modifierCastChecksAppliers.ContainsKey(id);

		/// <summary>
		///		Returns all modifiers back to the pool
		/// </summary>
		public void Clear()
		{
			foreach (var check in _modifierCastChecksAppliers.Values)
				ModifierPool.Instance.ReturnCheck(check);

			foreach (var check in _modifierAttackChecksAppliers.Values)
				ModifierPool.Instance.ReturnCheck(check);

			//Clear the rest, if the unit will be reused/pooled. Otherwise this is not needed
			_modifierAttackAppliers.Clear();
			_modifierCastAppliers.Clear();
			_modifierCastChecksAppliers.Clear();
			_modifierAttackChecksAppliers.Clear();
			_effectCasts.Clear();
		}
	}
}