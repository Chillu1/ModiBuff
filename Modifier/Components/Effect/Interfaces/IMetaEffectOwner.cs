namespace ModiBuff.Core
{
	/// <summary>
	///		Marker/helper interface (currently, might change in the future) for effects that can have meta effects.
	/// </summary>
	public interface IMetaEffectOwner<out TEffect, out TValueIn, in TReturnValue> //TODO Rename?
	{
		TEffect SetMetaEffects(params IMetaEffect<TValueIn, TReturnValue>[] metaEffects);
	}

	public interface IMetaEffectOwner<out TEffect, out TValueIn, out TValueIn2, in TReturnValue> //TODO Rename?
	{
		TEffect SetMetaEffects(params IMetaEffect<TValueIn, TValueIn2, TReturnValue>[] metaEffects);
	}
}