namespace ModiBuff.Core
{
	public interface IModifierStateInfo<out TState> where TState : struct
	{
		TState GetEffectData();
	}
}