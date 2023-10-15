namespace ModiBuff.Core
{
	public interface IModifierStateInfo
	{
	}

	/// <summary>
	///		Interface for effects that have state information, used for UI/UX
	/// </summary>
	/// <typeparam name="TData"></typeparam>
	public interface IModifierStateInfo<out TData> : IModifierStateInfo where TData : struct
	{
		TData GetEffectData();
	}
}