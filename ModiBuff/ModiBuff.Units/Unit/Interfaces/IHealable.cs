using System;

namespace ModiBuff.Core.Units
{
	public interface IHealable<THealth, TReturnHealthInfo> : IUnit
	{
		TReturnHealthInfo Heal(THealth heal, IUnit source, bool triggersEvents = true);
	}
}