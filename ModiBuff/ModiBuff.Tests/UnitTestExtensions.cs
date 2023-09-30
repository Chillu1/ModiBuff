using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	internal static class UnitTestExtensions
	{
		internal static float AttackN(this IAttacker<float, float> unit, IUnit target, int n)
		{
			var updatableUnit = unit as IUpdatable;
			var updatableTarget = target as IUpdatable;
			float totalDamage = 0;
			for (int i = 0; i < n; i++)
			{
				updatableUnit?.Update(0);
				updatableTarget?.Update(0);
				totalDamage += unit.Attack(target);
			}

			return totalDamage;
		}

		internal static float HealN(this IHealer<float, float> unit, IHealable<float, float> target, int n)
		{
			var updatableUnit = unit as IUpdatable;
			var updatableTarget = target as IUpdatable;
			float totalHeal = 0;
			for (int i = 0; i < n; i++)
			{
				updatableUnit?.Update(0);
				updatableTarget?.Update(0);
				totalHeal += unit.Heal(target);
			}

			return totalHeal;
		}
	}
}