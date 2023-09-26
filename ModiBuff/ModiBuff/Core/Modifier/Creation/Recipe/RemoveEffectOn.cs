using System;

namespace ModiBuff.Core
{
	[Flags]
	public enum RemoveEffectOn
	{
		None = 0,
		Stack = EffectOn.Stack,
		Callback = EffectOn.Callback,
	}

	public static class RemoveEffectOnExtensions
	{
		public static EffectOn ToEffectOn(this RemoveEffectOn removeEffectOn) => (EffectOn)removeEffectOn;
	}
}