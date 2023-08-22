using System;

namespace ModiBuff.Core
{
	public interface IAttacker<TDamage> where TDamage : IComparable<TDamage>
	{
		TDamage Damage { get; }

		TDamage Attack(IUnit target, bool triggersEvents = true);
	}
}