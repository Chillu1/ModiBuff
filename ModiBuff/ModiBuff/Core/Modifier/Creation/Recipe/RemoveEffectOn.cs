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

		CallbackUnit2 = EffectOn.CallbackUnit2,
		CallbackUnit3 = EffectOn.CallbackUnit3,
		CallbackUnit4 = EffectOn.CallbackUnit4,

		CallbackEffect2 = EffectOn.CallbackEffect2,
		CallbackEffect3 = EffectOn.CallbackEffect3,
		CallbackEffect4 = EffectOn.CallbackEffect4,
	}

	public static class RemoveEffectOnExtensions
	{
		public static EffectOn ToEffectOn(this RemoveEffectOn removeEffectOn) => (EffectOn)removeEffectOn;
	}
}