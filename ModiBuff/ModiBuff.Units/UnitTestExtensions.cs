namespace ModiBuff.Core.Units
{
	internal static class UnitTestExtensions
	{
		internal static float AttackN(this IAttacker<Damage> unit, IUnit target, int n)
		{
			float totalDamage = 0;
			for (int i = 0; i < n; i++)
				totalDamage += unit.Attack(target).Value;
			return totalDamage;
		}

		internal static float TakeDamage(this IDamagable<Damage> damagable, float damage, IUnit source, bool triggersEvents = true)
		{
			return damagable.TakeDamage(new Damage(damage), source, triggersEvents).Value;
		}
	}
}