namespace ModiBuff.Core
{
	public interface IPostEffectOwner<TValue> //TODO Rename
	{
		IEffect SetPostEffects(params IPostEffect<TValue>[] postEffects);
	}
}