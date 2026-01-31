using System;

namespace ModiBuff.Core.Units
{
	public sealed class StatCheck : IUnitCheck
	{
		private readonly StatType _statType;
		private readonly ComparisonType _comparisonType;
		private readonly float _statValue;

		public StatCheck(StatType statType, ComparisonType comparisonType, float statValue)
		{
			_statType = statType;
			_comparisonType = comparisonType;
			_statValue = statValue;
		}

		public bool Check(IUnit source)
		{
			float value = _statType switch
			{
				StatType.Health => ((IDamagable<float, float>)source).Health,
				StatType.MaxHealth => ((IDamagable<float, float>)source).MaxHealth,
				StatType.Mana => ((IManaOwner<float, float>)source).Mana,
				StatType.Damage => ((IAttacker<float, float>)source).Damage,
				_ => throw new ArgumentOutOfRangeException()
			};

			return _comparisonType.Check(value, _statValue);
		}
	}
}