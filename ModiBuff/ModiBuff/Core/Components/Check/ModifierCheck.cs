namespace ModiBuff.Core
{
	public sealed class ModifierCheck : IStateReset
	{
		public int Id { get; }
		public string Name { get; }

		private readonly bool _hasCondition, _hasCooldown, _hasCost, _hasChance, _hasCustomChecks, _hasCustomUsableChecks;

		private readonly ConditionCheck _condition;
		private readonly CooldownCheck _cooldown;
		private readonly CostCheck _cost;
		private readonly ChanceCheck _chance;
		private readonly ICheck[] _customChecks;
		private readonly IUsableCheck[] _usableChecks;

		public ModifierCheck(int id, string name, ConditionCheck condition = null, CooldownCheck cooldown = null, CostCheck cost = null,
			ChanceCheck chance = null, ICheck[] customChecks = null, IUsableCheck[] customUsableChecks = null)
		{
			Id = id;
			Name = name;

			_condition = condition;
			_cooldown = cooldown;
			_cost = cost;
			_chance = chance;
			_customChecks = customChecks;
			_usableChecks = customUsableChecks;

			_hasCondition = condition != null;
			_hasCooldown = cooldown != null;
			_hasCost = cost != null;
			_hasChance = chance != null;
			_hasCustomChecks = customChecks != null && customChecks.Length > 0;
			_hasCustomUsableChecks = customUsableChecks != null && customUsableChecks.Length > 0;
		}

		public void Update(float delta)
		{
			if (!_hasCooldown)
				return;

			_cooldown.Update(delta);
		}

		public bool Check(IUnit unit)
		{
			if (_hasCondition && !_condition.Check(unit))
				return false;
			if (_hasCooldown && !_cooldown.IsReady)
				return false;
			if (_hasCost && !_cost.Check(unit))
				return false;
			if (_hasChance && !_chance.Roll())
				return false;
			if (_hasCustomChecks)
				for (int i = 0; i < _customChecks.Length; i++)
				{
					if (!_customChecks[i].Check(unit))
						return false;
				}

			if (_hasCustomUsableChecks)
				for (int i = 0; i < _usableChecks.Length; i++)
				{
					if (!_usableChecks[i].Check(unit))
						return false;
				}

			if (_hasCooldown)
				_cooldown.ResetCooldown();
			if (_hasCost)
				_cost.Use(unit);
			if (_hasCustomUsableChecks)
				for (int i = 0; i < _usableChecks.Length; i++)
					_usableChecks[i].Use(unit);

			return true;
		}

		public void ResetState()
		{
			if (_hasCooldown)
				_cooldown.ResetCooldown();
		}
	}
}