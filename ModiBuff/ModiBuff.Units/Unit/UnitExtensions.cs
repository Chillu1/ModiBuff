namespace ModiBuff.Core.Units
{
	public static class UnitExtensions
	{
		public static float TakeDamage(this IUnit unit, float damage, IUnit source)
		{
			if (unit is IAttackable<float, float> damagableTarget)
				return damagableTarget.TakeDamage(damage, source);

			if (unit is IAttackable<int, int> damagableTargetInt)
				return damagableTargetInt.TakeDamage((int)damage, source);

			return 0;
		}
	}
}