using System;

namespace ModiBuff.Core.Units
{
	public interface IHealer<THealth, TReturnHealthInfo> where THealth : IComparable<THealth>
	{
		THealth HealValue { get; }

		TReturnHealthInfo Heal(IHealable<THealth, TReturnHealthInfo> target, bool triggersEvents = true);
	}
}