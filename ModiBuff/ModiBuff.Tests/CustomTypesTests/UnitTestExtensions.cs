using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests.CustomTypesTests
{
	internal static class UnitTestExtensions
	{
		internal static double AttackN(this IAttacker<Damage, double> unit, IUnit target, int n)
		{
			double totalDamage = 0;
			for (int i = 0; i < n; i++)
				totalDamage += unit.Attack(target);
			return totalDamage;
		}

		internal static double TakeDamage(this IDamagable<double, double, Damage, double> damagable, float damage, IUnit source,
			bool triggersEvents = true)
		{
			return damagable.TakeDamage(new Damage(damage), source, triggersEvents);
		}
	}
}