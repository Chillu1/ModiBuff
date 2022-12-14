using UnityEngine;

namespace ModifierLibraryLite.Core
{
	public sealed class ModifierCheck
	{
		public int IntId { get; }
		public string Name { get; }

		private readonly bool _hasCooldown, _hasCost, _hasChance;

		private readonly CooldownCheck _cooldown;
		private readonly CostCheck _cost;
		private readonly ChanceCheck _chance;

		public ModifierCheck(int intId, string name, CooldownCheck cooldown = null, CostCheck cost = null, ChanceCheck chance = null)
		{
			IntId = intId;
			Name = name;
			_cooldown = cooldown;
			_cost = cost;
			_chance = chance;

			_hasCooldown = _cooldown != null;
			_hasCost = _cost != null;
			_hasChance = _chance != null;
		}

		public void Update(in float delta)
		{
			if (!_hasCooldown)
				return;

			_cooldown.Update(delta);
		}

		public bool Check(IUnit unit)
		{
			//Debug.Log($"Checking {Name}");
			if (_hasCooldown && !_cooldown.IsReady)
				return false;
			if (_hasCost && !_cost.Check(unit))
				return false;
			if (_hasChance && !_chance.Roll())
				return false;

			if (_hasCooldown)
				_cooldown.ResetCooldown();
			if (_hasCost)
				_cost.Use(unit);

			return true;
		}
	}
}