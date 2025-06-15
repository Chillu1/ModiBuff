namespace ModiBuff.Core
{
	public interface IEffectStateInfo
	{
		object GetEffectData();
	}

	/// <summary>
	///		Interface for effects that have state information, used for UI/UX
	/// </summary>
	/// <typeparam name="TData"></typeparam>
	public interface IEffectStateInfo<out TData> : IEffectStateInfo
	{
		new TData GetEffectData();
	}
}