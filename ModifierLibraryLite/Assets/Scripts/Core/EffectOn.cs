using System;

namespace ModifierLibraryLite.Core
{
	[Flags]
	public enum EffectOn
	{
		None = 0,
		Init = 1,
		Interval = 2,
		Duration = 4,
		Refresh = 8,
		Stack = 16,
	}
}