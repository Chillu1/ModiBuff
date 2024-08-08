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
		CallbackUnit2 = 128,
		CallbackUnit3 = 256,
		CallbackUnit4 = 512,
		CallbackEffect2 = 1024,
		CallbackEffect3 = 2048,
		CallbackEffect4 = 4096,
		CallbackEffectUnits2 = 8192,
		CallbackEffectUnits3 = 16384,
		CallbackEffectUnits4 = 32768,
	}

	public static class EffectOnExtensions
	{
		public static RemoveEffectOn ToRemoveEffectOn(this EffectOn effectOn) => (RemoveEffectOn)effectOn;
	}

	public static class EffectOnCallbackEffectData
	{
		public static readonly EffectOn[] AllCallbackUnitData =
		{
			EffectOn.CallbackUnit,
			EffectOn.CallbackUnit2,
			EffectOn.CallbackUnit3,
			EffectOn.CallbackUnit4,
		};

		public static readonly EffectOn[] AllCallbackEffectData =
		{
			EffectOn.CallbackEffect,
			EffectOn.CallbackEffect2,
			EffectOn.CallbackEffect3,
			EffectOn.CallbackEffect4,
		};

		public static readonly EffectOn[] AllCallbackEffectUnitsData =
		{
			EffectOn.CallbackEffectUnits,
			EffectOn.CallbackEffectUnits2,
			EffectOn.CallbackEffectUnits3,
			EffectOn.CallbackEffectUnits4,
		};
	}
}