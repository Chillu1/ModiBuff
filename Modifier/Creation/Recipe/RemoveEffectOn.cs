using System;

namespace ModiBuff.Core
{
	[Flags]
	public enum RemoveEffectOn
	{
		None = 0,
		Stack = EffectOn.Stack,
		CallbackUnit = EffectOn.CallbackUnit,
		CallbackEffect = EffectOn.CallbackEffect,
	}

	public static class RemoveEffectOnExtensions
	{
		public static EffectOn ToEffectOn(this RemoveEffectOn removeEffectOn) => (EffectOn)removeEffectOn;
	}
}