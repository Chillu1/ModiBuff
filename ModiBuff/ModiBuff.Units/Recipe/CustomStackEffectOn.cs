namespace ModiBuff.Core.Units
{
	public enum CustomStackEffectOn
	{
		CallbackUnit = EffectOn.CallbackUnit,
		CallbackEffect = EffectOn.CallbackEffect,
		CallbackEffectUnits = EffectOn.CallbackEffectUnits,
		CallbackUnit2 = EffectOn.CallbackUnit2,
		CallbackUnit3 = EffectOn.CallbackUnit3,
		CallbackUnit4 = EffectOn.CallbackUnit4,
		CallbackEffect2 = EffectOn.CallbackEffect2,
		CallbackEffect3 = EffectOn.CallbackEffect3,
		CallbackEffect4 = EffectOn.CallbackEffect4,
		CallbackEffectUnits2 = EffectOn.CallbackEffectUnits2,
		CallbackEffectUnits3 = EffectOn.CallbackEffectUnits3,
		CallbackEffectUnits4 = EffectOn.CallbackEffectUnits4,
	}

	public static class CustomStackEffectOnExtensions
	{
		public static EffectOn ToEffectOn(this CustomStackEffectOn effectOn) => (EffectOn)effectOn;
	}
}