using System;

namespace ModiBuff.Core.Units
{
	public interface IHealer<THealth> where THealth : IComparable<THealth>
	{
		THealth HealValue { get; }

		THealth Heal(IHealable<THealth> target, bool triggersEvents = true);
	}
}