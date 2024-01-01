namespace ModiBuff.Core
{
	public interface IModifierDataReference
	{
		ITimeComponent[] GetTimers();
		ITimeReference GetTimer<TTimeComponent>(int timeComponentNumber = 0) where TTimeComponent : ITimeComponent;
		IStackReference GetStackReference();
		TData GetEffectState<TData>(int stateNumber = 0) where TData : struct;
	}
}