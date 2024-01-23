using System;

namespace ModiBuff.Core
{
	[Flags]
	public enum EffectOn
	{
		None = 0,
		Init = 1,
		Interval = 2,
		Duration = 4,
		Stack = 8,
		Event = 16,
		CallbackUnit = 32,
		CallbackEffect = 64,
		CallbackEffectUnits = 128,
	}
}