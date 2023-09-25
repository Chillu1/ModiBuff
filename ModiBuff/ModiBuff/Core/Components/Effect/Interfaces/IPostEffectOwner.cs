namespace ModiBuff.Core
{
	/// <summary>
	///		Marker/helper interface (currently, might change in the future) for effects that can have post effects.
	/// </summary>
	public interface IPostEffectOwner<out TEffect, out TValue> //TODO Rename
	{
		TEffect SetPostEffects(params IPostEffect<TValue>[] postEffects);
	}
}