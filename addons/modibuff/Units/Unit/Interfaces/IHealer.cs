using System;

namespace ModiBuff.Core.Units
{
	public interface IHealer<out THealth, TReturnHealthInfo> where THealth : IComparable<THealth>
	{
		THealth HealValue { get; }

		TReturnHealthInfo Heal(IHealable<THealth, TReturnHealthInfo> target);
	}
}