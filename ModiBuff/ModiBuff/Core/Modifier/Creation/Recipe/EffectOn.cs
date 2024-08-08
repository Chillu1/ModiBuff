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
		CallbackUnit = 16,
		CallbackEffect = 32,
		CallbackEffectUnits = 64,
		CallbackEffect2 = 128,
		CallbackEffect3 = 256,
		CallbackEffect4 = 512,
	}

	public static class EffectOnExtensions
	{
		public static RemoveEffectOn ToRemoveEffectOn(this EffectOn effectOn) => (RemoveEffectOn)effectOn;
	}

	public static class EffectOnCallbackEffectData
	{
		public static readonly EffectOn[] AllCallbackEffectData =
		{
			EffectOn.CallbackEffect,
			EffectOn.CallbackEffect2,
			EffectOn.CallbackEffect3,
			EffectOn.CallbackEffect4,
		};
	}
}