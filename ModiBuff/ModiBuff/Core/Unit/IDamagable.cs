using System;

namespace ModiBuff.Core
{
	public interface IDamagable<THealth> : IUnit where THealth : IComparable<THealth>
	{
		THealth Health { get; }
		THealth MaxHealth { get; }
	}

	public interface IDamagable<THealth, TDamage> : IDamagable<THealth>
		where TDamage : IComparable<TDamage> where THealth : IComparable<THealth>
	{
		TDamage TakeDamage(TDamage damage, IUnit source, bool triggersEvents = true);
	}
}