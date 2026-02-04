namespace ModiBuff.Core
{
	public interface IModifierDataReference
	{
		ITimeComponent[] GetTimers();
		ITimeReference? GetTimer<TTimeComponent>(int timeComponentNumber = 0) where TTimeComponent : ITimeComponent;
		ITimeTickComponent[] GetTickTimers();
		ITimeTickReference? GetTickTimer<TTimeTickComponent>(int timeTickComponentNumber = 0)
			where TTimeTickComponent : ITimeTickComponent;
		IStackReference? GetStackReference();

		(EffectOn EffectOn, TData Data)? GetEffectState<TData>(int stateNumber = 0, EffectOn effectOn = EffectOn.None)
			where TData : struct;
	}
}