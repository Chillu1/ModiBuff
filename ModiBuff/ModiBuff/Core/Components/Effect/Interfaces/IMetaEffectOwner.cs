namespace ModiBuff.Core
{
	public interface IMetaEffectOwner<TValue> //TODO Rename
	{
		IEffect SetMetaEffects(params IMetaEffect<TValue>[] metaEffects);
	}
}