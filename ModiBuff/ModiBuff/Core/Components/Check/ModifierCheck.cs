namespace ModiBuff.Core
{
	public sealed class ModifierCheck : IStateReset
	{
		public int Id { get; }

		private readonly bool _hasCondition, _hasCooldown, _hasCost, _hasChance, _hasNoUnitChecks, _hasUnitChecks, _hasUsableChecks;

		private readonly ConditionCheck _condition;
		private readonly CooldownCheck _cooldown;
		private readonly ChanceCheck _chance;
		private readonly INoUnitCheck[] _noUnitChecks;
		private readonly IUnitCheck[] _unitChecks;
		private readonly IUsableCheck[] _usableChecks;

		public ModifierCheck(int id, ConditionCheck condition = null, CooldownCheck cooldown = null, ChanceCheck chance = null,
			INoUnitCheck[] noUnitChecks = null, IUnitCheck[] unitChecks = null, IUsableCheck[] usableChecks = null)
		{
			Id = id;

			_condition = condition;
			_cooldown = cooldown;
			_chance = chance;
			_noUnitChecks = noUnitChecks;
			_unitChecks = unitChecks;
			_usableChecks = usableChecks;

			_hasCondition = condition != null;
			_hasCooldown = cooldown != null;
			_hasChance = chance != null;
			_hasNoUnitChecks = noUnitChecks != null && noUnitChecks.Length > 0;
			_hasUnitChecks = unitChecks != null && unitChecks.Length > 0;
			_hasUsableChecks = usableChecks != null && usableChecks.Length > 0;
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
			if (_hasChance && !_chance.Roll())
				return false;
			if (_hasNoUnitChecks)
				for (int i = 0; i < _noUnitChecks.Length; i++)
				{
					if (!_noUnitChecks[i].Check())
						return false;
				}

			if (_hasUnitChecks)
				for (int i = 0; i < _unitChecks.Length; i++)
				{
					if (!_unitChecks[i].Check(unit))
						return false;
				}

			if (_hasUsableChecks)
				for (int i = 0; i < _usableChecks.Length; i++)
				{
					if (!_usableChecks[i].Check(unit))
						return false;
				}

			if (_hasCooldown)
				_cooldown.ResetCooldown();
			if (_hasUsableChecks)
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