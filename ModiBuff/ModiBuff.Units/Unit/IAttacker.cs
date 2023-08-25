using System;

namespace ModiBuff.Core.Units
{
	public interface IAttacker<TDamage> where TDamage : IComparable<TDamage>
	{
		TDamage Damage { get; }

		TDamage Attack(IUnit target, bool triggersEvents = true);
	}
}