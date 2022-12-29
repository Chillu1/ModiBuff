using UnityEngine;

namespace ModiBuff.Core
{
	public sealed class ModifierCheck
	{
		public int Id { get; }
		public string Name { get; }

		private readonly bool _hasCondition, _hasCooldown, _hasCost, _hasChance;

		private readonly ConditionCheck _condition;
		private readonly CooldownCheck _cooldown;
		private readonly CostCheck _cost;
		private readonly ChanceCheck _chance;

		public ModifierCheck(int id, string name, ConditionCheck condition = null, CooldownCheck cooldown = null, CostCheck cost = null,
			ChanceCheck chance = null)
		{
			Id = id;
			Name = name;

			_condition = condition;
			_cooldown = cooldown;
			_cost = cost;
			_chance = chance;

			_hasCondition = condition != null;
			_hasCooldown = cooldown != null;
			_hasCost = cost != null;
			_hasChance = chance != null;
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
			if (_hasCondition && !_condition.Check(unit))
				return false;
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