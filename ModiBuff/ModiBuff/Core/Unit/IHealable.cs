using System;

namespace ModiBuff.Core
{
	public interface IHealable<THealth> : IUnit where THealth : IComparable<THealth>
	{
		THealth Heal(THealth heal, IUnit source, bool triggersEvents = true);
	}
}