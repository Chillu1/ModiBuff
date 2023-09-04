using System;

namespace ModiBuff.Core.Units
{
	public interface IHealable<in THealth, out TReturnHealthInfo> : IUnit
	{
		TReturnHealthInfo Heal(THealth heal, IUnit source, bool triggersEvents = true);
	}
}