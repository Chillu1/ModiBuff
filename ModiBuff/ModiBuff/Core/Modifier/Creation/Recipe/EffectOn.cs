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
		Callback = 32,
		CustomCallback = 64,
	}
}