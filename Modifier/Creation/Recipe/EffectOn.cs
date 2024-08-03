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
		CallbackEffect2 = 256,
		CallbackEffect3 = 512,
		CallbackEffect4 = 1024,
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