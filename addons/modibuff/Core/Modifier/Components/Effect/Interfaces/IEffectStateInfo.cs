namespace ModiBuff.Core
{
	public interface IEffectStateInfo
	{
	}

	/// <summary>
	///		Interface for effects that have state information, used for UI/UX
	/// </summary>
	/// <typeparam name="TData"></typeparam>
	public interface IEffectStateInfo<out TData> : IEffectStateInfo where TData : struct
	{
		TData GetEffectData();
	}
}