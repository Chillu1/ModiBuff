using System;

namespace ModiBuff.Core.Units
{
	public sealed class ConditionCheck : IUnitCheck
	{
		private readonly ConditionType _conditionType;

		public ConditionCheck(ConditionType conditionType) => _conditionType = conditionType;

		public bool Check(IUnit source) => _conditionType switch
		{
			ConditionType.HealthIsFull => source is IDamagable<float, float> damagable &&
			                              ComparisonType.Equal.Check(damagable.Health, damagable.MaxHealth),
			ConditionType.ManaIsFull => source is IManaOwner<float, float> manaOwner &&
			                            ComparisonType.Equal.Check(manaOwner.Mana, manaOwner.MaxMana),
			ConditionType.ManaIsEmpty => source is IManaOwner<float, float> manaOwner2 &&
			                             ComparisonType.LessOrEqual.Check(manaOwner2.Mana, 0f),
			_ => throw new ArgumentOutOfRangeException()
		};
	}
}