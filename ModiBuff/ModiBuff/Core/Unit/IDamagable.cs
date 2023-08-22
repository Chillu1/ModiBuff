using System;

namespace ModiBuff.Core
{
	public interface IDamagable
	{
		float Health { get; }
		float MaxHealth { get; }
	}

	public interface IDamagable<TDamage> : IDamagable, IUnit where TDamage : IComparable<TDamage>
	{
		TDamage TakeDamage(TDamage damage, IUnit source, bool triggersEvents = true);
	}
}