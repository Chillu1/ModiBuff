namespace ModiBuff.Core
{
	public interface IModifierStateInfo
	{
	}

	public interface IModifierStateInfo<out TState> : IModifierStateInfo where TState : struct
	{
		TState GetEffectData();
	}
}